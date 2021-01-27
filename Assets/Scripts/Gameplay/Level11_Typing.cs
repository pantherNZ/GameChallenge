using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text;

[Serializable]
public class Interval : Pair<float, float> { }

public class Level11_Typing : BaseLevel
{
    // Static data
    [SerializeField] GameObject windowPrefab = null;
    [SerializeField] int numWindowsEasy = 8;
    [SerializeField] int numWindowsHard = 12;

    [SerializeField] Interval speedIntervalEasy = new Interval();
    [SerializeField] Interval speedIntervalHard = new Interval();

    [SerializeField] List<AudioClip> createWindowAudio = new List<AudioClip>();
    [SerializeField] AudioClip completeWindowAudio = null;

    // Dynamic data
    int numWindowsToSpawn = 0;

    class Window
    {
        public GameObject obj;
        public Text text;
        public string str;
        public int index;
    }
    List<Window> windows = new List<Window>();
    List<string> gameStrings = new List<string>();

    // Functions
    public override void OnStartLevel()
    {
        gameStrings = DataManager.Instance.GetGameString( "Level11_Random_Strings" ).Split( new [] { ", " }, StringSplitOptions.RemoveEmptyEntries ).ToList();
        numWindowsToSpawn = desktop.IsEasyMode() ? numWindowsEasy : numWindowsHard;

        CreateWindow();
    }

    private void CreateWindow()
    {
        numWindowsToSpawn--;

        var window = desktop.CreateWindow( "Information", windowPrefab, false, desktop.GetScreenBound( 125.0f, false ).RandomPosition() );
        var index = UnityEngine.Random.Range( 0, gameStrings.Count - 1 );

        windows.Add( new Window()
        {
            obj = window,
            text = window.GetComponentsInChildren<Text>()[2],
            str = gameStrings[index],
            index = 0
        } );

        UpdateText( windows.Back() );
        gameStrings.RemoveBySwap( index );
        desktop.PlayAudio( createWindowAudio.RandomItem() );

        if( numWindowsToSpawn > 0 )
        {
            var range = desktop.IsEasyMode() ? speedIntervalEasy : speedIntervalHard;
            Utility.FunctionTimer.CreateTimer( UnityEngine.Random.Range( range.First, range.Second ), CreateWindow );
        }
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
        if( Input.anyKeyDown )
        {
            bool checkRemove = false;

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

            if( checkRemove )
            {
                windows.RemoveAll( ( window ) => window.obj == null );

                if( windows.IsEmpty() )
                {
                    if( numWindowsToSpawn == 0 )
                        LevelFinished( 3.0f );
                    else
                        CreateWindow();
                }
            }
        }
    }

    private void UpdateText( Window window )
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

    protected override void Cleanup()
    {
        foreach( var window in windows )
            window.obj.Destroy();
        windows.Clear();
        gameStrings.Clear();
    }
}