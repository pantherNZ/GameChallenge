using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using System.Collections;

[Serializable]
public class Interval : Pair<float, float> { }

public class Level11_Typing : BaseLevel
{
    // Static data
    [SerializeField] GameObject windowPrefab = null;
    [SerializeField] int numWindowsEasy = 8;
    [SerializeField] int numWindowsHard = 12;
    [SerializeField] float moveSpeedEasy = 1.0f;
    [SerializeField] float moveSpeedHard = 5.0f;
    [SerializeField] Interval speedIntervalEasy = new Interval();
    [SerializeField] Interval speedIntervalHard = new Interval();

    [SerializeField] List<AudioClip> createWindowAudio = new List<AudioClip>();
    [SerializeField] AudioClip completeWindowAudio = null;

    // Dynamic data
    int numWindowsToSpawn = 0;

    class WindowData
    {
        public GameObject obj;
        public Text text;
        public string str;
        public int index;
    }
    List<WindowData> windows = new List<WindowData>();
    List<string> gameStrings = new List<string>();
    bool checkRemove;

    // Functions
    public override void OnStartLevel()
    {
        PopulateStrings();
        numWindowsToSpawn = desktop.IsEasyMode() ? numWindowsEasy : numWindowsHard;

        CreateWindow( true );

        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_11_Start" );
    }

    private void PopulateStrings()
    {
        gameStrings = DataManager.Instance.GetGameString( "Level11_Random_Strings" ).Split( new[] { ", " }, StringSplitOptions.RemoveEmptyEntries ).ToList();
    }

    private void CreateWindow( bool createTimer, Vector2? position_override = null )
    {
        if( !levelActive )
            return;

        numWindowsToSpawn--;

        if( numWindowsToSpawn == ( desktop.IsEasyMode() ? numWindowsEasy : numWindowsHard ) / 2 )
            SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_11_1" );

        var rect = ( desktop.transform as RectTransform ).rect;
       // var pos = ( desktop.GetScreenBound( 125.0f, false ).RandomPosition() + rect.size / 2.0f ).SetY( rect.size.y );
        var pos = position_override ?? desktop.GetScreenBound( 125.0f, false ).RandomPosition().SetY( rect.size.y / 2.0f );
        var window = desktop.CreateWindow( "Information", windowPrefab, false, pos, true );
        var index = UnityEngine.Random.Range( 0, gameStrings.Count - 1 );

       // var finishPos = pos.SetY( 0.0f );
        var finishPos = pos.SetY( -rect.size.y / 2.0f );
        var finishPosWorld = window.transform.position.SetY( -window.transform.position.y );
        var speed = ( desktop.IsEasyMode() ? moveSpeedEasy : moveSpeedHard );
        StartCoroutine( MoveWindow( window.transform, finishPosWorld, ( finishPosWorld - window.transform.position ).magnitude / speed ) );

        windows.Add( new WindowData()
        {
            obj = window,
            text = window.GetComponentsInChildren<Text>()[2],
            str = gameStrings[index],
            index = 0
        } );

        window.GetComponent<Window>().onClose += ( _ ) => 
        {
            checkRemove = true;
            ++numWindowsToSpawn;
            CreateWindow( false, desktop.GetScreenBound( 125.0f, false ).RandomPosition().SetY( ( window.transform as RectTransform ).localPosition.y ) );
        };

        UpdateText( windows.Back() );
        gameStrings.RemoveBySwap( index );

        if( gameStrings.IsEmpty() )
            PopulateStrings();

        desktop.PlayAudio( createWindowAudio.RandomItem() );

        if( numWindowsToSpawn > 0 && createTimer )
        {
            var range = desktop.IsEasyMode() ? speedIntervalEasy : speedIntervalHard;
            Utility.FunctionTimer.CreateTimer( UnityEngine.Random.Range( range.First, range.Second ), () => CreateWindow( true ) );
        }
    }

    public IEnumerator MoveWindow( Transform transform, Vector3 pos, float duration )
    {
        yield return Utility.InterpolatePosition( transform, pos, duration );

        if( transform != null )
            desktop.LevelFailed();
    }

    private readonly Dictionary<char, KeyCode> keycodeCache = new Dictionary<char, KeyCode>();
    private KeyCode GetKeyCode( char character )
    {
        if( keycodeCache.TryGetValue( character, out KeyCode code ) )
            return code;

        int alphaValue = character;
        code = ( KeyCode )Enum.Parse( typeof( KeyCode ), alphaValue.ToString() );
        keycodeCache.Add( character, code );
        return code;
    }

    protected override void OnLevelUpdate()
    {
        if( checkRemove )
        {
            checkRemove = false;
            windows.RemoveAll( ( window ) => window.obj == null );

            if( windows.IsEmpty() )
            {
                if( numWindowsToSpawn == 0 )
                    LevelFinished( 3.0f );
                else
                    CreateWindow( false );
            }
        }

        if( Input.anyKeyDown )
        {
            foreach( var window in windows )
            {
                if( Input.GetKeyDown( GetKeyCode( char.ToLower( window.str[window.index] ) ) ) ||
                    Input.GetKeyDown( GetKeyCode( char.ToUpper( window.str[window.index] ) ) ) )
                    {
                        window.index++;

                    if( window.index < window.str.Length )
                    {
                        UpdateText( window );
                    }
                    else
                    {
                        checkRemove = true;
                        desktop.DestroyWindow( window.obj );
                        window.obj = null;
                        desktop.PlayAudio( completeWindowAudio );
                    }
                }
            }
        }
    }

    private void UpdateText( WindowData window )
    {
        var text = window.str;

        if( text[window.index] == ' ' )
        {
            StringBuilder sb = new StringBuilder( text );
            sb[window.index] = '_';
            text = sb.ToString();
        }

        text = text.Insert( window.index + 1, "</color>" );
        text = text.Insert( window.index, "<color=orange>" );

        if( window.index > 0 )
        {
            text = text.Insert( window.index, "</color>" );
            text = text.Insert( 0, "<color=green>" );
        }

        window.text.text = text;
    }

    protected override void Cleanup( bool fromRestart )
    {
        foreach( var window in windows )
            window.obj.Destroy();
        windows.Clear();
        gameStrings.Clear();
    }
}