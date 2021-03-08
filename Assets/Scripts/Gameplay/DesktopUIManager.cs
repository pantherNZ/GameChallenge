using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System;

[System.Serializable]
public class DesktopIcon
{
    public string name;
    public Sprite icon;
}

public class DesktopUIManager : BaseLevel, Game.ISavableObject
{
    public Camera MainCamera { get; private set; }
    public GameObject Taskbar { get => taskBar; private set { } }
    public static DesktopUIManager Instance;

    [Header( "Levels" )]
    [SerializeField] List<BaseLevel> levels = new List<BaseLevel>();
    [SerializeField] int startingLevelId = 0;
    int highestLevelUnlocked;

    // UI stuff
    [Header( "UI" )]
    public RectTransform DesktopCanvas;
    [SerializeField] GameObject windowBasePrefab = null;
    [SerializeField] GameObject optionsWindow = null;
    [SerializeField] Dropdown optionsWindowResolutions = null;
    [SerializeField] Toggle optionsWindowFullscreen = null;
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
    [SerializeField] UITimeSetter dateTimeAdjuster = null;
    [SerializeField] CanvasGroup updateTimerCanvas = null;

    // Cameras
    [SerializeField] Camera blueScreenCamera = null;
    [SerializeField] Camera windowCameraPrefab = null;
    [SerializeField] RenderTexture windowCamRTPrefab = null;
    [SerializeField] Camera gameCompleteCamera = null;

    // Selection box
    [SerializeField] GameObject selectionBoxPrefab = null;
    GameObject selectionBox;
    Vector3? selectionStartPos;
    [HideInInspector] public bool desktopSelectionEnabled = true;

    // Desktop context menu
    [SerializeField] GameObject contextMenu = null;
    [HideInInspector] public bool contextMenuEnabled = true;
    [HideInInspector] public bool recyclingEnabled = true;

    // Shortcuts
    [SerializeField] Vector2Int gridCellSize = new Vector2Int( 100, 100 );

    public class Shortcut
    {
        private GameObject _shortcut;
        public GameObject shortcut { get => _shortcut; set { _shortcut = value; grid = _shortcut.GetComponent<LockToGrid>(); } }
        public LockToGrid grid;
        public GameObject physics;
        public Action<GameObject> onOpened;
    }

    [HideInInspector] public List<Shortcut> shortcuts = new List<Shortcut>();

    // Errors
    List<Texture2D> errorTextures = new List<Texture2D>();

    // Misc
    [Header( "Settings" )]
    public Vector3 windowCameraStartPosition = new Vector3( -14.62f, 0.0f, -10.0f );
    [SerializeField] float lifeLostDisplayTime = 3.0f;
    [SerializeField] float levelFailFadeOutTime = 2.0f;
    [SerializeField] float levelFailFadeInTime = 2.0f;
    public float restartGameFadeOutTime = 5.0f;
    [SerializeField] bool enabledAudio = true;
    [SerializeField] bool enabledSaveLoad = true;
    [SerializeField] int timeUntilUpdateSec = 500;
    [SerializeField] bool easyDifficulty = true;

    // Audio
    [Header("Audio")]
    new AudioSource audio;
    AudioSource music;
    int musicIdx = 0;
    [SerializeField] AudioClip difficultySelectionAudio = null;
    [SerializeField] List<AudioClip> recycleAudio = new List<AudioClip>();
    [SerializeField] AudioClip gameLostAudio = null;
    [SerializeField] AudioClip levelFailAudio = null;
    [SerializeField] List<AudioClip> musicTracks = new List<AudioClip>();
    [SerializeField] AudioClip updateStartedAudio = null;

    Vector3 physicsRootOffset = new Vector3( -20.0f, 0.0f, 0.0f );
    GameObject taskbarPhysics, leftWall, rightWall;

    List<Pair<Window, string>> windows = new List<Pair<Window, string>>();
    int lives = 3;
    int currentLevel;
    Utility.FunctionTimer difficultyTimer;
    DateTime currentTime;
    public DateTime CurrentTime { get => currentTime; private set { } }
    Vector2 lastScreenSize;
    int updateTimeLeftSec;
    UIProgressBar updateProgressBar;

    [System.Serializable]
    public class Flag
    {
        public GameObject flag;
        public int index;
        public bool blackAquireText;
    }

    [Header( "Flags" )]
    [SerializeField] List<Flag> flags = new List<Flag>();
    [SerializeField] GameObject flagPrefab = null;
    [SerializeField] GameObject flagAcquiredPrefab = null;
    [SerializeField] Text flagsFoundCreditsText = null;
    [SerializeField] AudioClip flagAcquiredAudio = null;
    List<int> flagsFound = new List<int>();

    void Awake()
    {
        lastScreenSize = new Vector2( Screen.width, Screen.height );
    }

    private void Start()
    {
        Instance = this;

        List<string> resOptions = new List<string>();
        optionsWindowResolutions.ClearOptions();
        foreach( var res in Screen.resolutions )
            resOptions.Add( string.Format( "{0}x{1}", res.width, res.height ) );

        optionsWindowResolutions.AddOptions( resOptions );

        var audioSources = GetComponents<AudioSource>();
        audio = audioSources[0];
        music = audioSources[1];
        var sliders = optionsWindow.GetComponentsInChildren<Slider>();
        audio.volume = sliders[0].value;
        music.volume = sliders[1].value;
        currentTime = DateTime.Now;
        blueScreenCamera.gameObject.SetActive( false );
        errorTextures = Resources.LoadAll( "Textures/Errors/", typeof( Texture2D ) ).Cast<Texture2D>().ToList();

        MainCamera = Camera.main;
        SetAudoEnabled( enabledAudio );
        contextMenu.GetComponent<BoxCollider2D>().enabled = false;

        updateTimerCanvas.SetVisibility( false );
        updateTimeLeftSec = timeUntilUpdateSec;
        updateProgressBar = updateTimerCanvas.GetComponentInChildren<UIProgressBar>();

        CreateShortcut( new DesktopIcon() { name = "Recycle Bin", icon = Utility.CreateSprite( Resources.Load<Texture2D>( "Textures/StartMenuIcons/Full_Recycle_Bin" ) ) }, new Vector2Int() );

        for( int i = 0; i < levels.Count; ++i )
        {
            levels[i].levelIdx = i;
            levels[i].desktop = this;
            if( i < levels.Count - 1 )
                levels[i].nextLevel = levels[i + 1];
        }

        if( enabledSaveLoad )
        {
            Game.SaveGameSystem.AddSaveableObject( this );
            Game.SaveGameSystem.folderName = string.Empty;
            Game.SaveGameSystem.LoadGame( "UGC" );
        }

        foreach( var( index, res ) in Screen.resolutions.Enumerate() )
        {
            if( res.width == Screen.currentResolution.width &&
                res.height == Screen.currentResolution.height &&
                res.refreshRate == Screen.currentResolution.refreshRate )
            {
                optionsWindowResolutions.value = index;
                break;
            }
        }

        optionsWindowFullscreen.isOn = Screen.fullScreen;

        Utility.FunctionTimer.CreateTimer( 0.1f, () =>
        {
            flagsFoundCreditsText.text = string.Format( "{0} / {1} Flags Found", flagsFound.Count, flags.Count );

            for( int i = 0; i < flags.Count; ++i )
            {
                if( flagsFound.Contains( flags[i].index ) )
                {
                    flags[i].flag.Destroy();
                }
                else
                {
                    int idx = i;

                    flags[i].flag.GetComponent<Button>().onClick.AddListener( () =>
                    {
                        flagsFound.Add( idx );

                        if( enabledSaveLoad )
                            Game.SaveGameSystem.SaveGame( "UGC" );

                        PlayAudio( flagAcquiredAudio );

                        var obj = Instantiate( flagAcquiredPrefab, flags[idx].flag.transform.parent );
                        obj.transform.position = flags[idx].flag.transform.position;
                        var colour = flags[idx].blackAquireText ? Color.black : Color.white;
                        obj.GetComponent<Text>().color = colour;
                        this.FadeToColour( obj.GetComponent<Text>(), colour.SetA( 0.0f ), 1.0f );
                        this.InterpolatePosition( obj.transform, obj.transform.position + new Vector3( 0.0f, 1.0f, 0.0f ), 1.0f );
                        Utility.FunctionTimer.CreateTimer( 1.0f, () => obj.Destroy() );
                        flags[idx].flag.Destroy();
                        flagsFoundCreditsText.text = string.Format( "{0} / {1} Flags Found", flagsFound.Count, flags.Count );
                    } );
                }
            }
        } );

        Utility.FunctionTimer.CreateTimer( 0.001f, GetLevel( startingLevelId ).StartLevel );

        dateTimeAdjuster.GetComponent<UITimeSetter>().OnTimeChangedEvent += ( _, oldT, newT ) =>
        {
            timeDateText.text = DateTime.Today.Add( newT ).ToString( "h:mm tt\nM/dd/yyyy", System.Globalization.CultureInfo.CreateSpecificCulture( "en-US" ) );
        };

        if( startingLevelId >= 7 ) // Earthquake
            StartCoroutine( desktop.RunTimer() );

        musicTracks.RandomShuffle();
        PlayMusic();
    }

    public GameObject CreateFlag( Vector2 offset, int index, bool blackText = false, bool startHidden = false, string text = "", bool setLocalPos = false )
    {
        var flag = Instantiate( flagPrefab, DesktopCanvas );
        if( setLocalPos )
            ( flag.transform as RectTransform ).localPosition = offset.ToVector3();
        else
            ( flag.transform as RectTransform ).anchoredPosition = offset.ToVector3();
        flags.Add( new Flag() { flag = flag, index = index, blackAquireText = blackText } );
        flags.Sort( ( x, y ) => x.index - y.index);
        flag.GetComponent<CanvasGroup>().SetVisibility( !startHidden );
        flag.GetComponentInChildren<Text>().text = text;
        flag.name = "Flag: " + index;
        return flag;
    }

    public void ShowDesktopFlag()
    {
        var found = flags.Find( ( x ) => x.index == 1 );
        if( found != null && found.flag != null )
            found.flag.GetComponent<CanvasGroup>().SetVisibility( true );
    }

    private void PlayMusic()
    {
        music.clip = musicTracks[musicIdx];
        music.Play();
        musicIdx = ( musicIdx + 1 ) % musicTracks.Count;
        Utility.FunctionTimer.CreateTimer( music.clip.length + 5.0f, PlayMusic );
    }

    public void Serialise( BinaryWriter writer )
    {
        writer.Write( Mathf.Max( startingLevelId, currentLevel ) );
        writer.Write( Mathf.Max( highestLevelUnlocked, currentLevel ) );
        writer.Write( easyDifficulty );
        writer.Write( music.volume );
        writer.Write( audio.volume );
        writer.Write( updateTimeLeftSec );

        writer.Write( Screen.currentResolution.width );
        writer.Write( Screen.currentResolution.height );
        writer.Write( Screen.currentResolution.refreshRate );
        writer.Write( Screen.fullScreen );

        writer.Write( flagsFound.Count );
        foreach( var flag in flagsFound )
            writer.Write( flag );
    }

    public void Deserialise( BinaryReader reader )
    {
        startingLevelId = reader.ReadInt32();
        highestLevelUnlocked = reader.ReadInt32();
        easyDifficulty = reader.ReadBoolean();
        music.volume = reader.ReadSingle();
        audio.volume = reader.ReadSingle();
        var sliders = optionsWindow.GetComponentsInChildren<Slider>();
        sliders[0].value = audio.volume;
        sliders[1].value = music.volume;
        updateTimeLeftSec = reader.ReadInt32();

        var resWidth = reader.ReadInt32();
        var resHeight = reader.ReadInt32();
        var refreshRate = reader.ReadInt32();
        var fullscreen = reader.ReadBoolean();
        Screen.SetResolution( resWidth, resHeight, fullscreen, refreshRate );

        int flags = reader.ReadInt32();
        for( int i = 0; i < flags; ++i )
            flagsFound.Add( reader.ReadInt32() );
    }

    public override void OnStartLevel()
    {
        easyDifficulty = true;

        GetComponent<CanvasGroup>().SetVisibility( true );
        Utility.FunctionTimer.CreateTimer( 2.0f, () =>
        {
            var str = DataManager.Instance.GetGameString( "Narrator_Level_1_DifficultySelect" );
            SubtitlesManager.Instance.AddSubtitle( str, 0, 0, ( _, selection ) =>
            {
                PlayAudio( difficultySelectionAudio );

                if( selection == "hard"  )
                {
                    easyDifficulty = false;
                    SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_1_DifficultySelectHard" ) );
                    Utility.FunctionTimer.CreateTimer( 3.0f, StartNextLevel );
                }
            } );
        }, "selectionDelay" );

        Utility.FunctionTimer.CreateTimer( 8.0f, () =>
        {
            SubtitlesManager.Instance.SelectOption( 0 );
        } );
        difficultyTimer = Utility.FunctionTimer.CreateTimer( 10.0f, StartNextLevel );
    }

    protected override void Cleanup( bool fromRestart )
    {
        base.Cleanup( fromRestart );

        if( difficultyTimer != null )
            difficultyTimer.Stop();
        Utility.FunctionTimer.StopTimer( "selectionDelay" );
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
        var rect = new Rect( xPos, yPos, width, height );

        //Debug.DrawLine( rect.BottomLeft(), rect.BottomRight(), Color.red, 5.0f );
        //Debug.DrawLine( rect.BottomRight(), rect.TopRight(), Color.red, 5.0f );
        //Debug.DrawLine( rect.TopRight(), rect.TopLeft(), Color.red, 5.0f );
        //Debug.DrawLine( rect.TopLeft(), rect.BottomLeft(), Color.red, 5.0f );

        return rect;
    }

    public Rect GetScreenCoordinates( RectTransform uiElement )
    {
        var worldCorners = new Vector3[4];
        uiElement.GetWorldCorners( worldCorners );
        var result = new Rect(
                      worldCorners[0].x,
                      worldCorners[0].y,
                      worldCorners[2].x - worldCorners[0].x,
                      worldCorners[2].y - worldCorners[0].y );
        return result;
    }

    public Rect GetScreenBound( float margin = 0.0f, bool includeStartBar = false )
    {
        //var sf = GetComponent<Canvas>().scaleFactor;
        //margin *= sf;

        //var screenSize = new Vector2( Screen.width, Screen.height );
        //var rect = new Rect( new Vector2(), screenSize );
        var rect = ( transform as RectTransform ).rect;
        //rect.x += ( rect.width / 2.0f + margin );
        //rect.y += ( rect.height / 2.0f + margin );
        rect.x += margin;
        rect.y += margin;
        //rect.width *= sf;// * transform.localScale.x;
        //rect.height *= sf;// * transform.localScale.y;
        rect.width -= margin * 2.0f;
        rect.height -= margin * 2.0f;
        
        if( !includeStartBar )
        {
            rect.height -= ( taskBar.transform as RectTransform ).rect.height;
            rect.y += ( taskBar.transform as RectTransform ).rect.height;
        }

        //Debug.DrawLine( MainCamera.ScreenToWorldPoint( rect.BottomLeft() ), MainCamera.ScreenToWorldPoint( rect.BottomRight() ), Color.red, 5.0f );
        //Debug.DrawLine( MainCamera.ScreenToWorldPoint( rect.BottomRight() ), MainCamera.ScreenToWorldPoint( rect.TopRight() ), Color.red, 5.0f );
        //Debug.DrawLine( MainCamera.ScreenToWorldPoint( rect.TopRight() ), MainCamera.ScreenToWorldPoint( rect.TopLeft() ), Color.red, 5.0f );
        //Debug.DrawLine( MainCamera.ScreenToWorldPoint( rect.TopLeft() ), MainCamera.ScreenToWorldPoint( rect.BottomLeft() ), Color.red, 5.0f );

        return rect;
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

    public void LevelFailed()
    {
        lives--;
        PlayAudio( levelFailAudio );
        levels[currentLevel].Clear();

        if( lives == 0 )
        {
            this.FadeToBlack( levelFailFadeOutTime );
            Utility.FunctionTimer.CreateTimer( 3.0f, GameOver );
        }
        else
        {
            var errorTexture = errorTextures.RandomItem();
            var window = CreateWindow( "Critical Error" );
            window.GetComponent<Window>().image.GetComponent<RawImage>().texture = errorTexture;
            //( window.transform as RectTransform ).sizeDelta = new Vector2( errorTexture.width, errorTexture.height );

            Utility.FunctionTimer.CreateTimer( lifeLostDisplayTime, () => this.FadeToBlack( levelFailFadeOutTime ) );
            Utility.FunctionTimer.CreateTimer( lifeLostDisplayTime + levelFailFadeOutTime + 0.5f, () =>
            {
                DestroyWindow( window );
                RestartLevel();
                this.FadeFromBlack( levelFailFadeInTime );
            } );
        }
    }

    public void GameOver()
    {
        // Already won? bail
        if( gameCompleteCamera.gameObject.activeSelf )
            return;

        blueScreenCamera.gameObject.SetActive( true );
        MainCamera.gameObject.SetActive( false );
        PlayAudio( gameLostAudio );    
    }

    public void FinishGame()
    {
        // Already lost? bail
        if( blueScreenCamera.gameObject.activeSelf )
            return;

        gameCompleteCamera.gameObject.SetActive( true );
        MainCamera.gameObject.SetActive( false );
    }

    public void RestartGame( int startLevel )
    {
        RestartGame( startLevel, restartGameFadeOutTime );
    }

    public void RestartGame( float fadeOutTime )
    {
        RestartGame( 0, fadeOutTime );
    }

    public void RestartGame( int startLevel, float fadeOutTime )
    {
        gameCompleteCamera.gameObject.SetActive( false );
        blueScreenCamera.gameObject.SetActive( false );
        MainCamera.gameObject.SetActive( true );
        SubtitlesManager.Instance.canvasGroup.SetVisibility( false );

        foreach( var level in levels )
            level.Clear();

        this.FadeToBlack( fadeOutTime );
        levels[currentLevel].Clear();
        startingLevelId = startLevel;
        if( enabledSaveLoad )
            Game.SaveGameSystem.SaveGame( "UGC" );

        Utility.FunctionTimer.CreateTimer( restartGameFadeOutTime, () =>
        {
            MainCamera.gameObject.SetActive( true );
            blueScreenCamera.gameObject.SetActive( false );
            SubtitlesManager.Instance.canvasGroup.SetVisibility( true );
            levels[startLevel].Restart();
        } );
    }

    public void CreateOptionsWindow( Vector3 position )
    {
        ( optionsWindow.transform as RectTransform ).anchoredPosition = position;
        optionsWindow.GetComponent<CanvasGroup>().SetVisibility( true );
        SetContextMenuVisibility( false );
    }

    public void CreateOptionsWindow()
    {
        CreateOptionsWindow( GetMousePosScreen() );
    }

    public void CreateHelpWindow()
    {
        ( helpWindow.transform as RectTransform ).anchoredPosition = GetMousePosScreen();
        helpWindow.GetComponent<CanvasGroup>().SetVisibility( true );
        SetContextMenuVisibility( false );
    }

    public GameObject CreateWindow( string title, bool destroyExisting = false, Vector2 offset = new Vector2(), bool setLocalPos = false )
    {
        return CreateWindow( title, windowBasePrefab, destroyExisting, offset, setLocalPos );
    }

    public GameObject CreateWindow( string title, GameObject windowPrefab, bool destroyExisting, Vector2 offset, bool setLocalPos = false )
    {
        if( destroyExisting )
            DestroyWindowByTitle( title );

        var window = Instantiate( windowPrefab, DesktopCanvas ).GetComponent<Window>();
        if( setLocalPos )
            ( window.transform as RectTransform ).localPosition = offset.ToVector3();
        else
            ( window.transform as RectTransform ).anchoredPosition = offset.ToVector3();

        var createCam = window.HasViewPort();
        window.Initialise( title, this, createCam ? Instantiate( windowCameraPrefab ) : null, createCam ? Instantiate( windowCamRTPrefab ) : null );
        windows.Add( new Pair<Window, string>( window, title ) );
        taskBar.transform.SetAsLastSibling();
        return window.gameObject;
    }

    public void DestroyWindowByTitle( string title )
    {
        windows.RemoveBySwap( ( pair ) =>
        {
            if( pair.Second == title )
                pair.First.DestroyObject();
            return pair.Second == title;
        } );
    }

    public void DestroyWindowByTitle( Window window )
    {
        if( window != null )
            DestroyWindowByTitle( window.GetTitle() );
    }

    public void DestroyWindow( GameObject window )
    {
        if( window != null )
            DestroyWindow( window.GetComponent<Window>() );
    }

    public void DestroyWindow( Window window )
    {
        if( window != null )
        {
            windows.RemoveBySwap( ( pair ) =>
        {
            if( pair.First == window )
                pair.First.DestroyObject();
            return pair.First == window;
        } );
        }
    }

    public Rect GetGridBounds()
    {
        var rect = ( transform as RectTransform ).rect;
        float height = ( Mathf.Floor( rect.height / gridCellSize.y ) - 0.5f ) * gridCellSize.y;
        return new Rect( rect.xMin, rect.yMin + rect.height - height, ( Mathf.Floor( rect.width / gridCellSize.x ) - 0.5f ) * gridCellSize.x, height );
    }

    public GameObject CreateShortcut( DesktopIcon icon, Vector2Int index, System.Action<GameObject> onOpened = null )
    {
        return CreateShortcut( icon, GetGridBounds().TopLeft() + new Vector2( index.x * gridCellSize.x, -index.y * gridCellSize.y ), onOpened );
    }

    public GameObject CreateShortcut( DesktopIcon icon, Vector2 position, System.Action<GameObject> onOpened = null )
    {
        if( icon == null )
            return null;

        var newShortcut = Instantiate( shortcutPrefab, DesktopCanvas );
        ( newShortcut.transform as RectTransform ).localPosition = position - new Vector2( 0.0f, gridCellSize.y / 2.0f );
        ( newShortcut.transform as RectTransform ).pivot = new Vector2( 0.5f, 0.5f );
        newShortcut.GetComponentInChildren<Text>().text = icon.name;
        newShortcut.GetComponentsInChildren<Image>()[1].sprite = icon.icon;
        newShortcut.name = icon.name;

        newShortcut.AddComponent<EventDispatcher>().OnDoubleClickEvent += ( x ) =>
        {
            onOpened?.Invoke( newShortcut );
        };

        var grid = newShortcut.GetComponent<LockToGrid>();
        grid.cellWidth = gridCellSize.x;
        grid.cellHeight = gridCellSize.y;
        var bounds = GetGridBounds();
        grid.rootPos = bounds.TopLeft();
        grid.MinPos = bounds.min;
        grid.MaxPos = bounds.max;
        grid.MaxPos = new Vector2( grid.MaxPos.x, grid.MaxPos.y - gridCellSize.y / 2.0f );

        if( !shortcuts.IsEmpty() )
        {
            grid.onOverlapWith += ( obj ) =>
            {
                // Recycling bin
                if( recyclingEnabled && obj == shortcuts[0].shortcut )
                {
                    RemoveShortcut( newShortcut );
                    PlayAudio( recycleAudio.RandomItem() );
                }
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

        leftWall = Utility.CreateWorldObjectFromScreenSpaceRect( new Rect( -desktopRect.width / 2.0f, 0.0f, 0.1f, desktopRect.height ) );
        leftWall.transform.position = leftWall.transform.position + physicsRootOffset;
        leftWall.AddComponent<Quad>();
        leftWall.AddComponent<BoxCollider2D>().size = new Vector2( 1.0f, 1.0f );

        rightWall = Utility.CreateWorldObjectFromScreenSpaceRect( new Rect( desktopRect.width / 2.0f, 0.0f, 0.1f, desktopRect.height ) );
        rightWall.transform.position = rightWall.transform.position + physicsRootOffset;
        rightWall.AddComponent<Quad>();
        rightWall.AddComponent<BoxCollider2D>().size = new Vector2( 1.0f, 1.0f );
    }

    public void RemovePhysicsBound()
    {
        if( taskbarPhysics == null )
            return;
        taskbarPhysics.Destroy();
        leftWall.Destroy();
        rightWall.Destroy();
    }

    public Shortcut GetShortcut( int index )
    {
        Debug.Assert( index >= 0 && index < shortcuts.Count );
        return shortcuts[index];
    }

    private void Update()
    {
        if( Input.GetKeyDown( KeyCode.Escape ) )
            CreateOptionsWindow( new Vector3( MainCamera.pixelWidth / 2.0f, MainCamera.pixelHeight / 2.0f, 1.0f ) );

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

                if( Input.GetMouseButtonDown( 0 ) && !pointerTarget.transform.IsChildOf( dateTimeAdjuster.transform ) && pointerTarget != timeDateText.transform.parent && !pointerTarget.transform.IsChildOf( timeDateText.transform.parent ) )
                    dateTimeAdjuster.GetComponent<CanvasGroup>().SetVisibility( false );
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
        var newTime = DateTime.Now;

        if( newTime.Second != currentTime.Second )
        {
            timeDateText.text = newTime.ToString( "h:mm tt\nM/dd/yyyy", System.Globalization.CultureInfo.CreateSpecificCulture( "en-US" ) );
            var oldTime = currentTime.TimeOfDay;
            currentTime = newTime;
            var modifiedTime = dateTimeAdjuster.Time;
            dateTimeAdjuster.Time = currentTime.TimeOfDay + ( modifiedTime - oldTime );
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
                RestartGame( 0, 0.0f );

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
                    shortcut.grid.MinPos = bounds.min;
                    shortcut.grid.MaxPos = bounds.max;
                }
            }
        }

        updateProgressBar.Progress = ( timeUntilUpdateSec - updateTimeLeftSec ) / ( float )timeUntilUpdateSec;
    }

    public TimeSpan GetVirtualTime()
    {
        return dateTimeAdjuster.Time;
    }

    public Vector3 GetMousePosScreen()
    {
        var pos = new Vector3( 
            Input.mousePosition.x / MainCamera.pixelWidth * ( transform as RectTransform ).rect.width, 
            Input.mousePosition.y / MainCamera.pixelHeight * ( transform as RectTransform ).rect.height, 10.0f );

        var centre = new Vector3( MainCamera.scaledPixelWidth, MainCamera.scaledPixelHeight, 0.0f ) / 2.0f;
        return ( pos - centre ).RotateZ( DesktopCanvas.rotation.eulerAngles.z ) + centre;
    }

    public void SetContextMenuVisibility( bool visible )
    {
        contextMenu.GetComponent<CanvasGroup>().SetVisibility( visible );
        contextMenu.GetComponent<BoxCollider2D>().enabled = visible;
    }

    public bool ContextMenuVisibile()
    {
        return contextMenu.GetComponent<CanvasGroup>().IsVisible();
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

    public void ResetSpoiler( string spoilerTextGameString )
    {
        helpWindowSpoilerText.GetComponent<CanvasGroup>().SetVisibility( false );
        helpWindowSpoilerText.text = string.Empty;

        if( levels[currentLevel].spoilerTextGameString.Length > 0 )
            helpWindowSpoilerText.text = DataManager.Instance.GetGameString( spoilerTextGameString );

        helpWindowSpoilerButton.interactable = helpWindowSpoilerText.text.Length > 0;
    }

    public void LevelStarted( int level )
    {
        currentLevel = level;
        if( enabledSaveLoad )
            Game.SaveGameSystem.SaveGame( "UGC" );

        ResetSpoiler( levels[currentLevel].spoilerTextGameString );

        for( int i = 0; i < startMenuList.transform.childCount; ++i )
            startMenuList.transform.GetChild( i ).gameObject.Destroy();

        startMenuList.transform.DetachChildren();

        for( int i = 0; i <= Mathf.Max( highestLevelUnlocked, currentLevel ); ++i )
        {
            var icon = levels[i].startMenuEntryIcon;

            if( icon == null )
                continue;

            var entry = Instantiate( startMenuEntryPrefab, startMenuList.transform );
            entry.GetComponentInChildren<Text>().text = levels[i].startMenuEntryText;
            entry.GetComponentsInChildren<Image>()[1].sprite = icon;
            int levelIdx = i;
            entry.GetComponent<Button>().onClick.AddListener( () =>
            {
                RestartGame( levelIdx );
                ToggleStartMenuVisibility();
            } );
        }
    }

    public void LevelFinished( int level )
    {

    }

    public void PlayAudio( AudioClip clip, float volume = 1.0f )
    { 
        if( clip )
            audio.PlayOneShot( clip, volume );
    }

    public void ToggleDateTimeUI()
    {
        dateTimeAdjuster.GetComponent<CanvasGroup>().ToggleVisibility();
    }

    public void ToggleStartMenuVisibility()
    {
        startMenu.ToggleVisibility();
    }

    public void StartMenuButtonEnter( Image image )
    {
        image.color = new Color( 0.29f, 0.64f, 1.0f );
    }

    public void StartMenuButtonExit( Image image )
    {
        image.color = Color.white;
    }

    public IEnumerator RunTimer()
    {
        updateTimerCanvas.SetVisibility( true );
        desktop.PlayAudio( updateStartedAudio );

        StartCoroutine( RunTimerFlash( 8, 0.3f ) );

        while( updateTimeLeftSec > 0 )
        {
            updateTimeLeftSec--;
            updateTimerCanvas.GetComponentInChildren<Text>().text = DataManager.Instance.GetGameStringFormatted( "System_Update", new[] { TimeSpan.FromSeconds( updateTimeLeftSec ).ToString( @"mm\:ss" ) } );

            if( updateTimeLeftSec == 10 )
                StartCoroutine( RunTimerFlash( 10, 1.0f ) );

            yield return new WaitForSeconds( 1.0f );
        }

        GameOver();
    }

    public IEnumerator RunTimerFlash( int numFlashes, float flashInterval )
    {
        for( int i = 0; i < numFlashes; ++i )
        {
            yield return Utility.FadeToColour( updateTimerCanvas.GetComponentInChildren<Text>(), Color.red, flashInterval / 2.0f );
            yield return Utility.FadeToColour( updateTimerCanvas.GetComponentInChildren<Text>(), Color.white, flashInterval / 2.0f );
        }
    }

    public void SetAudioVolume( float scale )
    {
        audio.volume = scale;
        Game.SaveGameSystem.SaveGame( "UGC" );
    }

    public void SetMusicVolume( float scale )
    {
        music.volume = scale;
        Game.SaveGameSystem.SaveGame( "UGC" );
    }

    public void SetMusicVolume( Slider slider )
    {
        SetMusicVolume( slider.value );
    }

    public void SetAudioVolume( Slider slider )
    {
        SetAudioVolume( slider.value );
    }

    public void SetAudoEnabled( bool enabled )
    {
        MainCamera.GetComponent<AudioListener>().enabled = enabled;
    }

    public void SetAudoEnabled( Toggle toggle )
    {
        SetAudoEnabled( toggle.isOn );
    }

    public void UpdateResolution()
    {
        if( optionsWindowResolutions.value >= Screen.resolutions.Length )
        {
            Screen.fullScreen = optionsWindowFullscreen.isOn;
            Game.SaveGameSystem.SaveGame( "UGC" );
            return;
        }

        var res = Screen.resolutions[optionsWindowResolutions.value];
        var fullscreen = optionsWindowFullscreen.isOn;
        Screen.SetResolution( res.width, res.height, fullscreen, res.refreshRate );
        Game.SaveGameSystem.SaveGame( "UGC" );
    }

    public void RestartLevel()
    {
        levels[currentLevel].Restart();
    }

    public void DeleteSave()
    {
        Game.SaveGameSystem.DeleteSave( "UGC" );
    }

    public void SortDesktopIcons()
    {
        recyclingEnabled = false;

        foreach( var shortcut in shortcuts )
            shortcut.grid.SetGridPosition( new Vector2Int( shortcut.grid.gridWidth, shortcut.grid.gridHeight ) );

        foreach( var( index, shortcut ) in shortcuts.Enumerate() )
            shortcut.grid.SetGridPosition( new Vector2Int( index / shortcut.grid.gridWidth, -index % shortcut.grid.gridHeight ) );

        recyclingEnabled = true;
    }
}