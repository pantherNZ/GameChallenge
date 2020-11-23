using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using System.IO;

[System.Serializable]
public class DesktopIcon
{
    public string name;
    public Texture2D icon;
}

public class DesktopUIManager : BaseLevel, Game.ISavableObject
{
    public Camera MainCamera { get; private set; }
    public Vector3 windowCameraStartPosition = new Vector3( -14.62f, 0.0f, -10.0f );
    public GameObject Taskbar { get => taskBar; private set { } }
    public RectTransform DesktopCanvas;

    [SerializeField] List<BaseLevel> levels = new List<BaseLevel>();
    [SerializeField] int startingLevelId = 0;

    // UI stuff
    [SerializeField] LoginUI loginUI = null;
    [SerializeField] GameObject windowBasePrefab = null;
    [SerializeField] GameObject optionsWindow = null;
    [SerializeField] GameObject helpWindow = null;
    [SerializeField] Text helpWindowSpoilerText = null;
    [SerializeField] Button helpWindowSpoilerButton = null;
    [SerializeField] GameObject shortcutPrefab = null;
    [SerializeField] CanvasGroup startMenu = null;
    [SerializeField] GameObject taskBar = null;
    [SerializeField] GameObject startMenuButton = null;
    [SerializeField] Text timeDateText = null;
    [SerializeField] GameObject background = null;
    [SerializeField] GameObject startMenuEntryPrefab = null;
    [SerializeField] GameObject startMenuList = null;

    // Cameras
    [SerializeField] Camera blueScreenCamera = null;
    [SerializeField] Camera windowCameraPrefab = null;
    [SerializeField] RenderTexture windowCamRTPrefab = null;

    // Selection box
    [SerializeField] GameObject selectionBoxPrefab = null;
    GameObject selectionBox;
    Vector3? selectionStartPos;
    [HideInInspector] public bool desktopSelectionEnabled = true;

    // Desktop context menu
    [SerializeField] GameObject contextMenu = null;
    [HideInInspector] public bool contextMenuEnabled = true;

    // Shortcuts
    [SerializeField] Vector2Int gridSize = new Vector2Int( 100, 100 );

    public class Shortcut
    {
        private GameObject _shortcut;
        public GameObject shortcut { get => _shortcut; set { _shortcut = value; grid = _shortcut.GetComponent<LockToGrid>(); } }
        public LockToGrid grid;
        public GameObject physics;
        public System.Action<GameObject> onOpened;
    }

    [HideInInspector] public List<Shortcut> shortcuts = new List<Shortcut>();

    // Errors
    List<Texture2D> errorTextures = new List<Texture2D>();

    // Misc
    [SerializeField] float lifeLostDisplayTime = 3.0f;
    [SerializeField] float levelFailFadeOutTime = 2.0f;
    [SerializeField] float levelFailFadeInTime = 2.0f;
    [SerializeField] float restartGameFadeOutTime = 5.0f;
    [SerializeField] float restartGameFadeInTime = 2.0f;
    [SerializeField] bool enabledAudio = true;

    Vector3 physicsRootOffset = new Vector3( -20.0f, 0.0f, 0.0f );
    GameObject taskbarPhysics;

    List<Pair<Window, string>> windows = new List<Pair<Window, string>>();
    [SerializeField] bool easyDifficulty = true;
    int lives = 3;
    int currentLevel;
    Utility.FunctionTimer difficultyTimer;
    System.DateTime currentTime;
    Vector2 lastScreenSize;

    void Awake()
    {
        lastScreenSize = new Vector2( Screen.width, Screen.height );
    }

    private void Start()
    {
        blueScreenCamera.gameObject.SetActive( false );
        errorTextures = Resources.LoadAll( "Textures/Errors/", typeof( Texture2D ) ).Cast<Texture2D>().ToList();

        MainCamera = Camera.main;
        MainCamera.GetComponent<AudioListener>().enabled = enabledAudio;
        contextMenu.GetComponent<BoxCollider2D>().enabled = false;

        CreateShortcut( new DesktopIcon() { name = "Recycle Bin", icon = Resources.Load<Texture2D>( "Textures/Full_Recycle_Bin" ) }, new Vector2Int() );

        for( int i = 0; i < levels.Count; ++i )
        {
            levels[i].levelIdx = i;
            levels[i].desktop = this;
            if( i < levels.Count - 1 )
                levels[i].nextLevel = levels[i + 1];
        }

        Game.SaveGameSystem.AddSaveableObject( this );
        Game.SaveGameSystem.folderName = string.Empty;
        Game.SaveGameSystem.LoadGame( "UGC" );

        Utility.FunctionTimer.CreateTimer( 0.001f, GetLevel( startingLevelId ).StartLevel );
    }

    public void Serialise( BinaryWriter writer )
    {
        writer.Write( Mathf.Max( startingLevelId, currentLevel ) );
    }

    public void Deserialise( BinaryReader reader )
    {
        startingLevelId = reader.ReadInt32();
    }

    public override void OnStartLevel()
    {
        GetComponent<CanvasGroup>().SetVisibility( true );
        Utility.FunctionTimer.CreateTimer( 2.0f, () =>
        {
            var str = DataManager.Instance.GetGameString( "Narrator_Level_1_DifficultySelect" );
            SubtitlesManager.Instance.AddSubtitle( str, 0, 0, ( selection ) =>
            {
                if( selection == "hard" )
                {
                    easyDifficulty = false;

                    if( !difficultyTimer.active )
                        SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_1_DifficultySelectHard" ) );
                    Utility.FunctionTimer.CreateTimer( 3.0f, StartNextLevel );
                }
            } );
        } );

        difficultyTimer = Utility.FunctionTimer.CreateTimer( 5.0f, () =>
        {
            if( !easyDifficulty )
                SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_1_DifficultySelectHard" ) );
            Utility.FunctionTimer.CreateTimer( 3.0f, StartNextLevel );
        } );
    }

    public BaseLevel GetLevel( int index )
    {
        if( index < 0 || index >= levels.Count || levels[index] == null )
        {
            Debug.LogError( "LoadingUI: forceStartLevelId invalid value: " + index.ToString() );
            return null;
        }

        return levels[index];
    }

    public Rect GetWorldBound( float margin = 0.0f, bool includeStartBar = false )
    {
        var startBarHeight = ( taskBar.transform as RectTransform ).rect.height;
        var startOffset = includeStartBar ? 0.0f : ( ( startBarHeight / MainCamera.pixelHeight ) * MainCamera.orthographicSize * 2.0f );
        var width = MainCamera.aspect * MainCamera.orthographicSize * 2.0f - margin * 2.0f;
        var height = MainCamera.orthographicSize * 2.0f - margin * 2.0f - startOffset;
        var xPos = -width / 2.0f;
        var yPos = -height / 2.0f + startOffset / 2.0f;
        return new Rect( xPos, yPos, width, height );
    }

    public bool IsEasyMode()
    {
        return easyDifficulty;
    }

    public void CloseGame()
    {
        // Save
        Application.Quit();
    }

    public void LevelFailed( BaseLevel level )
    {
        lives--;

        if( lives == 0 )
        {
            Utility.FunctionTimer.CreateTimer( 3.0f, GameOver );
        }
        else
        {
            var errorTexture = errorTextures.RandomItem();
            var window = CreateWindow( "Critical Error" );
            window.GetComponent<Window>().image.GetComponent<RawImage>().texture = errorTexture;
            ( window.transform as RectTransform ).sizeDelta = new Vector2( errorTexture.width, errorTexture.height );

            Utility.FunctionTimer.CreateTimer( lifeLostDisplayTime, () => this.FadeToBlack( levelFailFadeOutTime ) );
            Utility.FunctionTimer.CreateTimer( lifeLostDisplayTime + levelFailFadeOutTime, () =>
            {
                this.FadeFromBlack( levelFailFadeInTime );
            } );
            Utility.FunctionTimer.CreateTimer( lifeLostDisplayTime + levelFailFadeOutTime + levelFailFadeInTime, () =>
            {
                level.StartLevel();
                DestroyWindow( window );
            } );
        }
    }

    public void GameOver()
    {
        blueScreenCamera.gameObject.SetActive( true );
        MainCamera.gameObject.SetActive( false );
    }

    public void RestartGame()
    {
        this.FadeToBlack( restartGameFadeOutTime );

        Utility.FunctionTimer.CreateTimer( restartGameFadeOutTime, () =>
        {
            this.FadeFromBlack( restartGameFadeInTime );
            MainCamera.gameObject.SetActive( true );
            blueScreenCamera.gameObject.SetActive( false );
            loginUI.StartLevel();
        } );
    }

    public GameObject CreateWindow( string title, bool destroyExisting = false, Vector2 offset = new Vector2() )
    {
        return CreateWindowInternal( title, windowBasePrefab, destroyExisting, offset );
    }

    public void CreateOptionsWindow()
    {
        ( optionsWindow.transform as RectTransform ).anchoredPosition = GetMousePosScreen();
        optionsWindow.GetComponent<CanvasGroup>().SetVisibility( true );
        SetContextMenuVisibility( false );
    }

    public void CreateHelpWindow()
    {
        ( helpWindow.transform as RectTransform ).anchoredPosition = GetMousePosScreen();
        helpWindow.GetComponent<CanvasGroup>().SetVisibility( true );
        SetContextMenuVisibility( false );
    }

    private GameObject CreateWindowInternal( string title, GameObject windowPrefab, bool destroyExisting, Vector2 offset )
    {
        if( destroyExisting )
            DestroyWindow( title );

        var window = Instantiate( windowPrefab, DesktopCanvas ).GetComponent<Window>();
        ( window.transform as RectTransform ).anchoredPosition = offset.ToVector3();
        var createCam = window.HasViewPort();
        window.Initialise( title, this, createCam ? Instantiate( windowCameraPrefab ) : null, createCam ? Instantiate( windowCamRTPrefab ) : null );
        windows.Add( new Pair<Window, string>( window, title ) );
        taskBar.transform.SetAsLastSibling();
        return window.gameObject;
    }

    public void DestroyWindow( string title )
    {
        windows.RemoveBySwap( ( pair ) =>
        {
            if( pair.Second == title )
                pair.First.DestroyObject();
            return pair.Second == title;
        } );
    }

    public void DestroyWindow( Window window )
    {
        DestroyWindow( window.GetTitle() );
    }

    public void DestroyWindow( GameObject window )
    {
        DestroyWindow( window.GetComponent<Window>().GetTitle() );
    }

    public Rect GetGridBounds()
    {
        var rect = ( transform as RectTransform ).rect;
        float height = ( Mathf.Floor( rect.height / gridSize.y ) - 0.5f ) * gridSize.y;
        return new Rect( rect.xMin, rect.yMin + rect.height - height, ( Mathf.Floor( rect.width / gridSize.x ) - 0.5f ) * gridSize.x, height );
    }

    public GameObject CreateShortcut( DesktopIcon icon, Vector2Int index, System.Action<GameObject> onOpened = null )
    {
        return CreateShortcut( icon, GetGridBounds().TopLeft() + new Vector2( index.x * gridSize.x, -index.y * gridSize.y ), onOpened );
    }

    public GameObject CreateShortcut( DesktopIcon icon, Vector2 position, System.Action<GameObject> onOpened = null )
    {
        if( icon == null )
            return null;

        var newShortcut = Instantiate( shortcutPrefab, DesktopCanvas );
        ( newShortcut.transform as RectTransform ).localPosition = position - new Vector2( 0.0f, gridSize.y / 2.0f );
        ( newShortcut.transform as RectTransform ).pivot = new Vector2( 0.5f, 0.5f );
        newShortcut.GetComponentInChildren<Text>().text = icon.name;
        newShortcut.GetComponentsInChildren<Image>()[1].sprite = Sprite.Create( icon.icon, new Rect( 0.0f, 0.0f, icon.icon.width, icon.icon.height ), new Vector2( 0.5f, 0.5f ) );
        newShortcut.name = icon.name;

        newShortcut.AddComponent<EventDispatcher>().OnDoubleClickEvent += ( x ) =>
        {
            onOpened?.Invoke( newShortcut );
        };

        var grid = newShortcut.GetComponent<LockToGrid>();
        grid.gridWidth = gridSize.x;
        grid.gridHeight = gridSize.y;
        var bounds = GetGridBounds();
        grid.rootPos = bounds.TopLeft();
        grid.minPos = bounds.min;
        grid.maxPos = bounds.max;
        grid.maxPos.y -= gridSize.y / 2.0f;

        if( !shortcuts.IsEmpty() )
        {
            grid.onOverlapWith += ( obj ) =>
            {
                // Recycling bin
                if( obj == shortcuts[0].shortcut )
                    RemoveShortcut( newShortcut );
            };
        }

        shortcuts.Add( new Shortcut() { shortcut = newShortcut, onOpened = onOpened } );
        FixChildOrdering();

        return newShortcut;
    }

    public void RemoveShortcut( GameObject shortcut )
    {
        if( shortcut == null )
            return;

        var idx = shortcuts.FindIndex( ( x ) => x.shortcut == shortcut );

        if( idx == -1 )
            return;

        shortcuts[idx].physics?.Destroy();
        shortcuts[idx].shortcut?.Destroy();
        shortcuts.RemoveBySwap( idx );
    }

    public GameObject ShortcutAddPhysics( GameObject shortcut )
    {
        if( shortcut == null )
            return null;

        var idx = shortcuts.FindIndex( ( x ) => x.shortcut == shortcut );

        if( idx == -1 )
            return null;

        if( shortcuts[idx].physics != null )
            return null;

        var physics = Utility.CreateWorldObjectFromScreenSpaceRect( ( shortcut.transform as RectTransform ).GetWorldRect() );
        physics.transform.position = physics.transform.position + physicsRootOffset;
        physics.AddComponent<Quad>();
        physics.AddComponent<BoxCollider2D>().size = new Vector2( 1.0f, 1.0f );
        physics.AddComponent<Rigidbody2D>();
        shortcuts[idx].physics = physics;
        shortcuts[idx].shortcut.GetComponent<LockToGrid>().enabled = false;
        shortcuts[idx].shortcut.GetComponent<EventTrigger>().enabled = false;
        return physics;
    }

    public void ShortcutRemovePhysics( GameObject shortcut, bool relock_to_grid = false )
    {
        if( shortcut == null )
            return;

        var idx = shortcuts.FindIndex( ( x ) => x.shortcut == shortcut );

        if( idx == -1 )
            return;

        shortcuts[idx].shortcut.GetComponent<LockToGrid>().enabled = relock_to_grid;
        shortcuts[idx].shortcut.GetComponent<EventTrigger>().enabled = relock_to_grid;
        shortcuts[idx].physics?.Destroy();
    }

    public void CreatePhysicsBound()
    {
        if( taskbarPhysics != null )
            return;

        taskbarPhysics = Utility.CreateWorldObjectFromScreenSpaceRect( ( Taskbar.transform as RectTransform ).GetWorldRect() );
        taskbarPhysics.transform.position = taskbarPhysics.transform.position + physicsRootOffset;
        taskbarPhysics.AddComponent<Quad>();
        taskbarPhysics.AddComponent<BoxCollider2D>().size = new Vector2( 1.0f, 1.0f );

        var desktopRect = ( transform as RectTransform ).GetWorldRect();

        var leftWall = Utility.CreateWorldObjectFromScreenSpaceRect( new Rect( -desktopRect.width / 2.0f, 0.0f, 0.1f, desktopRect.height ) );
        leftWall.transform.position = leftWall.transform.position + physicsRootOffset;
        leftWall.AddComponent<Quad>();
        leftWall.AddComponent<BoxCollider2D>().size = new Vector2( 1.0f, 1.0f );

        var rightWall = Utility.CreateWorldObjectFromScreenSpaceRect( new Rect( desktopRect.width / 2.0f, 0.0f, 0.1f, desktopRect.height ) );
        rightWall.transform.position = rightWall.transform.position + physicsRootOffset;
        rightWall.AddComponent<Quad>();
        rightWall.AddComponent<BoxCollider2D>().size = new Vector2( 1.0f, 1.0f );
    }

    public void RemovePhysicsBound()
    {
        if( taskbarPhysics == null )
            return;
        taskbarPhysics.Destroy();
    }

    public Shortcut GetShortcut( int index )
    {
        Debug.Assert( index >= 0 && index < shortcuts.Count );
        return shortcuts[index];
    }

    private void Update()
    {
        if( ( Input.GetMouseButtonDown( 0 ) || Input.GetMouseButtonDown( 1 ) ) && selectionStartPos == null )
        {
            var pointerData = new PointerEventData( EventSystem.current ) { pointerId = -1, position = Input.mousePosition };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll( pointerData, results );
            var pointerTarget = results.IsEmpty() ? null : results.Front().gameObject;

            if( pointerTarget != null )
            {
                if( startMenu.IsVisible() )
                {
                    while( pointerTarget != null && pointerTarget.transform.parent != null && pointerTarget != startMenu.gameObject && pointerTarget != startMenuButton.gameObject )
                        pointerTarget = pointerTarget.transform.parent.gameObject;

                    if( results.IsEmpty() || ( pointerTarget != startMenu.gameObject && pointerTarget != startMenuButton.gameObject ) )
                        startMenu.ToggleVisibility();
                }

                // Start selection box
                if( Input.GetMouseButtonDown( 0 ) && ( pointerTarget == null || pointerTarget == background ) )
                    selectionStartPos = GetMousePosScreen();

                if( Input.GetMouseButtonDown( 0 ) && !pointerTarget.transform.IsChildOf( contextMenu.transform ) )
                    SetContextMenuVisibility( false );
            }
        }

        // End selection box
        if( Input.GetMouseButtonUp( 0 ) )
        {
            selectionStartPos = null;
            if( selectionBox != null )
                selectionBox.Destroy();
        }

        // Selection box positioning
        if( selectionStartPos != null && GetMousePosScreen() != selectionStartPos && desktopSelectionEnabled )
        {
            if( selectionBox == null )
            {
                selectionBox = Instantiate( selectionBoxPrefab, DesktopCanvas );
                FixChildOrdering();
            }

            var difference = GetMousePosScreen() - selectionStartPos.Value;
            ( selectionBox.transform as RectTransform ).anchoredPosition = ( GetMousePosScreen() + selectionStartPos.Value ) / 2.0f;
            ( selectionBox.transform as RectTransform ).sizeDelta = new Vector2( Mathf.Abs( difference.x ), Mathf.Abs( difference.y ) );
        }

        // Time update on taskbar
        var newTime = System.DateTime.Now;

        if( newTime != currentTime )
        {
            currentTime = newTime;
            timeDateText.text = newTime.ToString( "h:mm tt\nM/dd/yyyy", System.Globalization.CultureInfo.CreateSpecificCulture( "en-US" ) );
        }

        // Context menu on desktop
        if( Input.GetMouseButtonDown( 1 ) && contextMenuEnabled )
        {
            SetContextMenuVisibility( true );
            ( contextMenu.transform as RectTransform ).anchoredPosition = GetMousePosScreen(); 
            ( contextMenu.transform as RectTransform ).pivot = new Vector2( 0.0f, GetMousePosScreen().y <= 160.0f ? 0.0f : 1.0f );
        }

        if( blueScreenCamera.gameObject.activeSelf )
            if( Input.anyKeyDown )
                RestartGame();

        for( int i = shortcuts.Count - 1; i >= 0; --i )
            if( shortcuts[i] == null )
                shortcuts.RemoveBySwap( i );

        Vector2 screenSize = new Vector2( Screen.width, Screen.height );

        if( lastScreenSize != screenSize )
        {
            lastScreenSize = screenSize;
            {
                foreach( var shortcut in shortcuts )
                {
                    var bounds = GetGridBounds();
                    shortcut.grid.rootPos = bounds.TopLeft();
                    shortcut.grid.minPos = bounds.min;
                    shortcut.grid.maxPos = bounds.max;
                }
            }
        }
    }

    public Vector3 GetMousePosScreen()
    {
        var centre = new Vector3( MainCamera.pixelWidth, MainCamera.pixelHeight, 0.0f ) / 2.0f;
        return ( Input.mousePosition - centre ).RotateZ( DesktopCanvas.rotation.eulerAngles.z ) + centre;
    }

    public void SetContextMenuVisibility( bool visible )
    {
        contextMenu.GetComponent<CanvasGroup>().SetVisibility( visible );
        contextMenu.GetComponent<BoxCollider2D>().enabled = visible;
    }

    private void LateUpdate()
    {
        foreach( var shortcut in shortcuts )
        {
            if( shortcut.physics != null )
            {
                var newPos = shortcut.physics.transform.position - physicsRootOffset;// + shortcut.Second.transform.localScale / 2.0f;
                ( shortcut.shortcut.transform as RectTransform ).localPosition = ( transform as RectTransform ).InverseTransformPoint( newPos ).SetZ( 0.0f );
                ( shortcut.shortcut.transform as RectTransform ).localRotation = shortcut.physics.transform.rotation;
            }
        }
    }

    private void FixChildOrdering()
    {
        if( selectionBox != null )
            selectionBox.transform.SetAsFirstSibling();

        foreach( var shortcut in shortcuts )
            shortcut.shortcut.transform.SetAsFirstSibling();

        background.transform.SetAsFirstSibling();
        startMenu.transform.parent.transform.SetAsLastSibling();
    }

    public GameObject GetBackground()
    {
        return background;
    }

    public void RevealSpoiler()
    {
        helpWindowSpoilerButton.interactable = false;
        helpWindowSpoilerText.GetComponent<CanvasGroup>().SetVisibility( true );
    }

    public void LevelStarted( int level )
    {
        currentLevel = level;
        Game.SaveGameSystem.SaveGame( "UGC" );
        helpWindowSpoilerText.GetComponent<CanvasGroup>().SetVisibility( false );
        helpWindowSpoilerText.text = DataManager.Instance.GetGameString( levels[currentLevel].spoilerTextGameString );
        helpWindowSpoilerButton.interactable = helpWindowSpoilerText.text.Length > 0;

        startMenuList.transform.DetachChildren();

        for( int i = 0; i <= Mathf.Max( startingLevelId, currentLevel ); ++i )
        {
            var icon = levels[i].startMenuEntryIcon;

            if( icon == null )
                continue;

            var entry = Instantiate( startMenuEntryPrefab, startMenuList.transform );
            entry.GetComponentInChildren<Text>().text = levels[i].startMenuEntryText;
            entry.GetComponentsInChildren<Image>()[1].sprite = Sprite.Create( icon, new Rect( 0.0f, 0.0f, icon.width, icon.height ), new Vector2( 0.5f, 0.5f ) );
        }
    }

    public void ShowDateTimeUI()
    {

    }

    public void ToggleStartMenuVisibility()
    {
        startMenu.ToggleVisibility();
    }
}