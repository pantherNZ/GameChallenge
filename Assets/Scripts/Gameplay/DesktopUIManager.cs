using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

[System.Serializable]
public class DesktopIcon
{
    public string name;
    public Texture2D icon;
}

public class DesktopUIManager : BaseLevel
{
    public Camera WindowCamera { get => windowCamera; private set { } }
    public Camera MainCamera { get; private set; }
    public Vector3 windowCameraStartPosition { get; private set; }
    public GameObject Taskbar { get => taskBar; private set { } }

    [SerializeField] List<BaseLevel> levels = new List<BaseLevel>();
    [SerializeField] int startingLevelId = 0;

    // UI stuff
    [SerializeField] LoginUI loginUI = null;
    [SerializeField] GameObject windowBase = null;
    [SerializeField] GameObject optionsWindow = null;
    [SerializeField] GameObject helpWindow = null;
    [SerializeField] GameObject shortcut = null;
    [SerializeField] CanvasGroup startMenu = null;
    [SerializeField] GameObject taskBar = null;
    [SerializeField] Button startMenuButton = null;
    [SerializeField] Text timeDateText = null;
    [SerializeField] GameObject background = null;

    // Cameras
    [SerializeField] Camera blueScreenCamera = null;
    [SerializeField] Camera windowCamera = null;

    // Selection box
    [SerializeField] GameObject selectionBoxPrefab = null;
    GameObject selectionBox;
    Vector3? selectionStartPos;
    [HideInInspector] public bool desktopSelectionEnabled = true;

    // Desktop context menu
    [SerializeField] GameObject contextMenu = null;

    // Shortcuts
    [SerializeField] Vector2Int gridSize = new Vector2Int( 100, 100 );
    [HideInInspector] public List<GameObject> shortcuts = new List<GameObject>();

    // Errors
    List<Texture2D> errorTextures = new List<Texture2D>();

    // Misc
    [SerializeField] float lifeLostDisplayTime = 3.0f;
    [SerializeField] float levelFailFadeOutTime = 2.0f;
    [SerializeField] float levelFailFadeInTime = 2.0f;
    [SerializeField] float restartGameFadeOutTime = 5.0f;
    [SerializeField] float restartGameFadeInTime = 2.0f;
    [SerializeField] bool enabledAudio = true;

    List<Pair<Window, string>> windows = new List<Pair<Window, string>>();
    bool easyDifficulty = true;
    int lives = 3;
    Utility.FunctionTimer difficultyTimer;
    System.DateTime currentTime;

    private void Start()
    {
        blueScreenCamera.gameObject.SetActive( false );
        errorTextures = Resources.LoadAll( "Textures/Errors/", typeof( Texture2D ) ).Cast<Texture2D>().ToList();

        startMenuButton.onClick.AddListener( () => { startMenu.ToggleVisibility(); } );
        windowCameraStartPosition = WindowCamera.transform.position.SetZ( 0.0f );
        MainCamera = Camera.main;
        MainCamera.GetComponent<AudioListener>().enabled = enabledAudio;
        contextMenu.GetComponent<BoxCollider2D>().enabled = false;

        CreateShortcut( new DesktopIcon() { name = "Recycle Bin", icon = Resources.Load<Texture2D>( "Textures/Full_Recycle_Bin" ) }, new Vector2Int() );

        for( int i = 0; i < levels.Count; ++i )
        {
            levels[i].levelIdx = i;
            if( i < levels.Count - 1 )
                levels[i].nextLevel = levels[i + 1];
        }

        Utility.FunctionTimer.CreateTimer( 0.001f, GetLevel( startingLevelId ).StartLevel );
    }

    public override void OnStartLevel()
    {
        GetComponent<CanvasGroup>().SetVisibility( true );
        Utility.FunctionTimer.CreateTimer( 2.0f, () => 
        {
            var str = DataManager.Instance.GetGameString( "Narrator_Level_2_DifficultySelect" );
            SubtitlesManager.Instance.AddSubtitle( str, 0, 0, ( selection ) =>
            {
                if( selection == "hard" )
                {
                    easyDifficulty = false;

                    if( !difficultyTimer.active )
                        SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_2_DifficultySelectHard" ) );
                    Utility.FunctionTimer.CreateTimer( 3.0f, StartNextLevel );
                }
            } );
        } );

        difficultyTimer = Utility.FunctionTimer.CreateTimer( 5.0f, () =>
        {
            if( !easyDifficulty )
                SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_2_DifficultySelectHard" ) );
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

    public GameObject CreateWindow( string title, bool destroyExisting = false )
    {
        return CreateWindowInternal( title, windowBase, destroyExisting );
    }

    public void CreateOptionsWindow()
    {
        CreateWindowInternal( "Options", optionsWindow, true );
    }

    public void CreateHelpWindow()
    {
        CreateWindowInternal( "Help", helpWindow, true );
    }

    private GameObject CreateWindowInternal( string title, GameObject windowPrefab, bool destroyExisting )
    {
        if( destroyExisting )
            DestroyWindow( title );

        var window = Instantiate( windowPrefab, transform ).GetComponent<Window>();
        window.transform.position = transform.position;
        window.Initialise( title, this );
        windows.Add( new Pair<Window, string>( window, title ) );
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

    public GameObject CreateShortcut( DesktopIcon icon, Vector2Int index )
    {
        return CreateShortcut( icon, GetGridBounds().TopLeft() + new Vector2( index.x * gridSize.x, -index.y * gridSize.y ) );
    }

    public GameObject CreateShortcut( DesktopIcon icon, Vector2 position )
    {
        if( icon == null )
            return null;

        var newShortcut = Instantiate( shortcut, transform );
        ( newShortcut.transform as RectTransform ).localPosition = position - new Vector2( 0.0f, gridSize.y / 2.0f );
        ( newShortcut.transform as RectTransform ).pivot = new Vector2( 0.5f, 0.5f );
        newShortcut.GetComponentInChildren<Text>().text = icon.name;
        newShortcut.GetComponentsInChildren<Image>()[1].sprite = Sprite.Create( icon.icon, new Rect( 0.0f, 0.0f, icon.icon.width, icon.icon.height ), new Vector2( 0.5f, 0.5f ) );

        var grid = newShortcut.GetComponent<LockToGrid>();
        grid.gridWidth = gridSize.x;
        grid.gridHeight = gridSize.y;
        var bounds = GetGridBounds();
        grid.rootPos = bounds.TopLeft();
        grid.minPos = bounds.min;
        grid.maxPos = bounds.max;

        grid.onOverlapWith += ( obj ) => 
        {
            if( obj == shortcuts[0] )
            {
                shortcuts.Remove( newShortcut );
                newShortcut.Destroy();
            }
        };

        shortcuts.Add( newShortcut );

        FixChildOrdering();

        return shortcuts.Back();
    }

    public GameObject GetShortcut( int index )
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

            if( startMenu.IsVisible() )
            {
                while( pointerTarget != null && pointerTarget.transform.parent != null && pointerTarget != startMenu.gameObject && pointerTarget != startMenuButton.gameObject )
                    pointerTarget = pointerTarget.transform.parent.gameObject;

                if( results.IsEmpty() || ( pointerTarget != startMenu.gameObject && pointerTarget != startMenuButton.gameObject ) )
                    startMenu.ToggleVisibility();
            }

            // Start selection box
            if( Input.GetMouseButtonDown( 0 ) && ( pointerTarget == null || pointerTarget == background ) )
                selectionStartPos = Input.mousePosition;

            if( Input.GetMouseButtonDown( 0 ) && !pointerTarget.transform.IsChildOf( contextMenu.transform ) )
            {
                contextMenu.GetComponent<CanvasGroup>().SetVisibility( false );
                contextMenu.GetComponent<BoxCollider2D>().enabled = false;
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
        if( selectionStartPos != null && Input.mousePosition != selectionStartPos && desktopSelectionEnabled )
        {
            if( selectionBox == null )
            {
                selectionBox = Instantiate( selectionBoxPrefab, transform );
                FixChildOrdering();
            }

            var difference = Input.mousePosition - selectionStartPos.Value;
            ( selectionBox.transform as RectTransform ).anchoredPosition = ( Input.mousePosition + selectionStartPos.Value ) / 2.0f;
            ( selectionBox.transform as RectTransform ).sizeDelta = new Vector2( Mathf.Abs( difference.x ), Mathf.Abs( difference.y ) );
        }

        // Time update on taskbar
        var newTime = System.DateTime.Now;

        if( newTime != currentTime )
        {
            currentTime = newTime;
            timeDateText.text = newTime.ToString( "h:mm tt\nM/dd/yyyy", System.Globalization.CultureInfo.CreateSpecificCulture( "en-US" ) );
        }

        var window = windows.IsEmpty() ? null : windows.Front().First;

        // Wiewport inside window
        if( window != null && window.HasViewPort() )
        {
            Vector3[] corners = new Vector3[4];
            window.GetCameraViewWorldCorners( corners );
            var windowViewPosWorld = ( corners[3] + corners[0] ) / 2.0f;
            var offset = windowViewPosWorld - transform.position;
            windowCamera.transform.position = windowCameraStartPosition + offset;
        }

        // Context menu on desktop
        if( Input.GetMouseButtonDown( 1 ) )
        {
            contextMenu.GetComponent<CanvasGroup>().SetVisibility( true );
            contextMenu.GetComponent<BoxCollider2D>().enabled = true;
            ( contextMenu.transform as RectTransform ).anchoredPosition = Input.mousePosition;
            ( contextMenu.transform as RectTransform ).pivot = new Vector2( 0.0f, Input.mousePosition.y <= 160.0f ? 0.0f : 1.0f );
        }

        if( blueScreenCamera.gameObject.activeSelf )
            if( Input.anyKeyDown )
                RestartGame();
    }

    private void FixChildOrdering()
    {
        if( selectionBox != null )
            selectionBox.transform.SetAsFirstSibling();

        foreach( var shortcut in shortcuts )
            shortcut.transform.SetAsFirstSibling();

        background.transform.SetAsFirstSibling();
        startMenu.transform.parent.transform.SetAsLastSibling();
    }

    public GameObject GetBackground()
    {
        return background;
    }
}
