using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class DesktopUIManager : MonoBehaviour
{
    [SerializeField] Camera windowCamera = null;
    public Camera WindowCamera { get => windowCamera; private set { } }
    public Camera MainCamera { get; private set; }
    public Vector3 windowCameraStartPosition { get; private set; }

    // UI stuff
    [SerializeField] GameObject windowBase = null;
    [SerializeField] GameObject optionsWindow = null;
    [SerializeField] GameObject helpWindow = null;
    [SerializeField] GameObject shortcut = null;
    [SerializeField] CanvasGroup startMenu = null;
    [SerializeField] Button startMenuButton = null;
    [SerializeField] Text timeDateText = null;
    [SerializeField] GameObject background = null;

    // Camera
    [SerializeField] Camera blueScreenCamera = null;

    // Selection box
    [SerializeField] GameObject selectionBoxPrefab = null;
    GameObject selectionBox;
    Vector3? selectionStartPos;

    // Desktop context menu
    [SerializeField] GameObject contextMenu = null;

    // Shortcuts
    [SerializeField] Vector2Int gridSize = new Vector2Int( 100, 100 );
    [HideInInspector] public List<GameObject> shortcuts = new List<GameObject>();

    List<Pair<Window, string>> windows = new List<Pair<Window, string>>();
    bool easyDifficulty = true;
    Utility.FunctionTimer difficultyTimer;
    System.DateTime currentTime;

    private void Start()
    {
        startMenuButton.onClick.AddListener( () => { startMenu.ToggleVisibility(); } );
        windowCameraStartPosition = WindowCamera.transform.position.SetZ( 0.0f );
        MainCamera = Camera.main;

        CreateShortcut( "Recycle Bin", Resources.Load< Texture2D >( "Textures/Full_Recycle_Bin" ), new Vector2Int() );
    }

    public bool IsEasyMode()
    {
        return easyDifficulty;
    }

    public void StartLevel()
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
                }
            } );
        } );

        difficultyTimer = Utility.FunctionTimer.CreateTimer( 5.0f, () =>
        {
            if( !easyDifficulty )
                SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_2_DifficultySelectHard" ) );
        } );
    }

    public void CloseGame()
    {
        // Save
        Application.Quit();
    }

    public void GameOver()
    {
        blueScreenCamera.gameObject.SetActive( true );
        MainCamera.gameObject.SetActive( false );
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

    public Rect GetGridBounds()
    {
        var rect = ( transform as RectTransform ).rect;
        return new Rect( 0.0f, 0.0f,
             ( Mathf.Floor( rect.width / gridSize.x ) - 0.5f ) * gridSize.x,
            -( Mathf.Floor( rect.height / gridSize.y ) - 0.5f ) * gridSize.y );
    }
    public GameObject CreateShortcut( string title, Texture2D icon, Vector2Int index )
    {
        return CreateShortcut( title, icon, ( index * gridSize ).ToVector2() );
    }

    public GameObject CreateShortcut( string title, Texture2D icon, Vector2 position )
    {
        var newShortcut = Instantiate( shortcut, transform );
        ( newShortcut.transform as RectTransform ).anchoredPosition = position;
        newShortcut.GetComponentInChildren<Text>().text = title;
        newShortcut.GetComponentsInChildren<Image>()[1].sprite = Sprite.Create( icon, new Rect( 0.0f, 0.0f, icon.width, icon.height ), new Vector2( 0.5f, 0.5f ) );
        startMenu.transform.parent.transform.SetAsLastSibling();

        var grid = newShortcut.GetComponent<LockToGrid>();
        grid.gridWidth = gridSize.x;
        grid.gridHeight = gridSize.y;
        var bounds = GetGridBounds();
        grid.minPos = new Vector2( bounds.x, bounds.height );
        grid.maxPos = new Vector2( bounds.width, bounds.y );

        grid.onOverlapWith += ( obj ) => 
        {
            if( obj == shortcuts[0] )
            {
                shortcuts.Remove( newShortcut );
                newShortcut.Destroy();
            }
        };

        shortcuts.Add( newShortcut );

        return newShortcut;
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
                contextMenu.GetComponent<CanvasGroup>().SetVisibility( false );
        }

        // End selection box
        if( Input.GetMouseButtonUp( 0 ) )
        {
            selectionStartPos = null;
            if( selectionBox != null )
                selectionBox.Destroy();
        }

        // Selection box positioning
        if( selectionStartPos != null && Input.mousePosition != selectionStartPos )
        {
            if( selectionBox == null )
            {
                selectionBox = Instantiate( selectionBoxPrefab, transform );
                selectionBox.transform.SetAsFirstSibling();
                background.transform.SetAsFirstSibling();
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

        if( window != null )
        {
            Vector3[] corners = new Vector3[4];
            window.GetCameraViewWorldCorners( corners );
            var windowViewPosWorld = ( corners[3] + corners[0] ) / 2.0f;
            var offset = windowViewPosWorld - transform.position;
            windowCamera.transform.position = windowCameraStartPosition + offset;
        }

        if( Input.GetMouseButtonDown( 1 ) )
        {
            contextMenu.GetComponent<CanvasGroup>().SetVisibility( true );
            ( contextMenu.transform as RectTransform ).anchoredPosition = Input.mousePosition;
            ( contextMenu.transform as RectTransform ).pivot = new Vector2( 0.0f, Input.mousePosition.y <= 160.0f ? 0.0f : 1.0f );
        }
    }
}
