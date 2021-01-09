﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubtitlesManager : MonoBehaviour
{
    [HideInInspector] public static SubtitlesManager Instance { get; private set; }
    [SerializeField] GameObject selectionGroup = null;

    string currentText = string.Empty;
    int index;
    float timer;
    float fadeOutTime, fadeOutDelay;
    public Text text = null;
    public bool appearInstantly;
    public float updateIntervalSec = 0.05f;
    public int selectionMargin = 4;

    public class Selection
    {
        public int index;
        public string first, second;
        public GameObject obj;
        public bool complete;
    }

    List<Selection> selections = new List<Selection>();

    [HideInInspector] public Action<string> onSelectionEvent;

    public class Timer
    {
        public int index;
        public int timerTextLength;
        public string timerName;
        public Utility.FunctionTimer timer;
    }

    List<Timer> timers = new List<Timer>();

    CanvasGroup canvasGroup;

    void Start()
    {
        Instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
        ClearSubtitles();
    }

    void Update()
    {
        if( index < currentText.Length )
        {
            timer += Time.deltaTime;

            if( timer >= updateIntervalSec )
            {
                timer -= updateIntervalSec;
                text.text = currentText.Substring( 0, index + 1 );

                // Increased length of full stops
                if( currentText[index] == '.' && ( index + 1 >= currentText.Length || currentText[index + 1] != '.' ) )
                {
                    timer -= updateIntervalSec * 10.0f;
                }
                else if( index < currentText.Length && currentText[index] == ',' )
                {
                    timer -= updateIntervalSec * 5.0f;
                }

                index++;

                if( index >= currentText.Length )
                    timer = 0.0f;
            }
        }
        else if( timer < fadeOutDelay + fadeOutTime )
        {
            timer += Time.deltaTime;

            if( timer >= fadeOutDelay )
            {
                canvasGroup.alpha = 1.0f - ( timer - fadeOutDelay ) / fadeOutTime;

                if( canvasGroup.alpha <= 0.0f )
                    ClearSubtitles();
            }
        }

        foreach( var ( idx, timer ) in timers.Enumerate() )
        {
            if( index >= timer.index && timer.timer != null )
                UpdateTimerText( timer );
            else if( timer.timer == null && timer.timerTextLength != 0 )
                UpdateTimerText( timer, string.Empty );

            if( timer.timer != null && timer.timer.timeLeft <= 0.0f )
            {
                timers.RemoveBySwap( idx );
                break;
            }
        }
    }

    public void AddSubtitleGameString( string gameString, float fadeOutDelay = 0.0f, float fadeOutTime = 0.0f, Action<string> onSelection = null )
    {
        AddSubtitle( DataManager.Instance.GetGameString( gameString ), fadeOutDelay, fadeOutTime, onSelection );
    }

    public void AddSubtitle( string subtitle, float fadeOutDelay = 0.0f, float fadeOutTime = 0.0f, Action<string> onSelection = null )
    {
        ClearSubtitles();
        onSelectionEvent = onSelection;
        CheckForSelections( ref subtitle );

        currentText = subtitle;
        index = 0;
        timer = 0.0f;
        this.fadeOutTime = fadeOutTime;
        this.fadeOutDelay = fadeOutDelay;
        canvasGroup.SetVisibility( true );

        if( appearInstantly )
        {
            text.text = currentText;
            index = currentText.Length;

            if( !selections.IsEmpty() )
            {
                var selection = selections.Front();
                selection.obj.SetActive( true );
                ( selection.obj.transform as RectTransform ).localPosition = GetCharacterPosition( text, selection.index + selectionMargin + Mathf.Max( selection.first.Length, selection.second.Length ) / 2 );
                var texts = selections.Front().obj.GetComponentsInChildren<Text>();
                texts[0].text = selections.Front().first;
                texts[1].text = selections.Front().second;
                var events = selections.Front().obj.GetComponentsInChildren<EventDispatcher>();

                Action<int, Color> updateColour = ( int index, Color colour ) =>
                {
                    if( !selection.complete && selections.Contains( selection ) )
                        texts[index].color = colour;
                };

                var selectionColour = new Color( 0.47f, 0.57f, 1.0f );

                events[0].OnPointerEnterEvent += ( x ) => { updateColour( 0, selectionColour ); };
                events[0].OnPointerExitEvent += ( x ) => { updateColour( 0, Color.white ); };
                events[1].OnPointerEnterEvent += ( x ) => { updateColour( 1, selectionColour ); };
                events[1].OnPointerExitEvent += ( x ) => { updateColour( 1, Color.white ); };

                Action< int > selectionEvent = ( int index ) =>
                {
                    if( !selection.complete )
                    {
                        selection.obj.transform.GetChild( 1 - index ).gameObject.Destroy();
                        updateColour( index, selectionColour );
                        selection.complete = true;
                        onSelectionEvent?.Invoke( text.text );
                    }
                };

                events[0].OnPointerDownEvent += ( x ) => { selectionEvent( 0 ); };
                events[1].OnPointerDownEvent += ( x ) => { selectionEvent( 1 ); };
            }
        }
    }

    public void QueueSubtitle( float delay, string subtitle, float fadeOutDelay = 0.0f, float fadeOutTime = 0.0f, Action<string> onSelection = null )
    {
        Utility.FunctionTimer.CreateTimer( delay, () =>
        {
            AddSubtitle( subtitle, fadeOutDelay, fadeOutTime, onSelection );
        }, subtitle );
    }

    public void QueueSubtitleGameString( float delay, string gameString, float fadeOutDelay = 0.0f, float fadeOutTime = 0.0f, Action<string> onSelection = null )
    {
        Utility.FunctionTimer.CreateTimer( delay, () =>
        {
            AddSubtitle( DataManager.Instance.GetGameString( gameString ), fadeOutDelay, fadeOutTime, onSelection );
        }, gameString );
    }

    public void ClearSubtitles()
    {
        text.text = currentText = string.Empty;
        canvasGroup.SetVisibility( false );
        selectionGroup.SetActive( false );

        foreach( var selection in selections )
            selection.obj?.Destroy();

        selections.Clear();
        timers.Clear();
    }

    void CheckForSelections( ref string subtitle )
    {
        for( int counter = 0; counter < subtitle.Length; ++counter )
        {
            var keyword = "<select=";
            var index = subtitle.IndexOf( keyword, counter );
            if( index != -1 )
            {
                int start = index;
                int startSelect = index + keyword.Length;
                index = subtitle.IndexOf( '|', index );

                if( index == -1 )
                    Debug.LogError( "AddSubtitle called with <select=.. keyword without selection '|' in subtitle: " + subtitle );

                int end = index;
                index = subtitle.IndexOf( '>', index );

                if( index == -1 )
                    Debug.LogError( "AddSubtitle called with <select=.. keyword without closing '>' in subtitle: " + subtitle );

                selections.Add( new Selection()
                {
                    index = start + 1,
                    first = subtitle.Substring( startSelect, end - startSelect ),
                    second = subtitle.Substring( end + 1, index - end - 1 ),
                    obj = Instantiate( selectionGroup, transform )
                } );

                subtitle = subtitle.Remove( start, index - start + 1 );
                subtitle = subtitle.Insert( start, new string( ' ', Mathf.Max( selections.Back().first.Length, selections.Back().second.Length ) + selectionMargin * 2 ) );
            }

            keyword = "<timer=";
            index = subtitle.IndexOf( keyword );
            if( index != -1 )
            {
                int end = subtitle.IndexOf( '>', index );

                if( index == -1 )
                    Debug.LogError( "AddSubtitle called with <timer=.. keyword without closing '>' in subtitle: " + subtitle );

                timers.Add( new Timer() { index = index, timerName = subtitle.Substring( index + keyword.Length, end - index - keyword.Length ) } );
                subtitle = subtitle.Remove( index, end - index + 1 );
            }
        }
    }

    public void AssignTimer( Utility.FunctionTimer timer )
    {
        var timerObj = timers.Find( ( x ) => x.timerName == timer.name );

        if( timerObj == null )
            Debug.LogError( "AssignTimer failed to find a matching timer object in the subtitle: " + timer.name );

        timerObj.timer = timer;
    }

    private void UpdateTimerText( Timer timer )
    {
        UpdateTimerText( timer, string.Format( "{0}s", timer.timer.timeLeft.ToString( "N1" ) ) );
    }

    private void UpdateTimerText( Timer timer, string newStr )
    {
        currentText = currentText.Remove( timer.index, timer.timerTextLength );
        currentText = currentText.Insert( timer.index, newStr );
        index += ( newStr.Length - timer.timerTextLength );
        text.text = index < currentText.Length ? currentText.Substring( 0, index + 1 ) : currentText;
        timer.timerTextLength = newStr.Length;
    }

    Vector3 GetCharacterPosition( Text text, int charIndex )
    {
        if( charIndex >= text.text.Length )
        {
            Debug.LogError( "GetCharacterPosition: Out of text bound" );
            return new Vector3();
        }

        string str = text.text.Replace( ' ', '.' );

        TextGenerator textGen = new TextGenerator( str.Length );
        Vector2 extents = text.gameObject.GetComponent<RectTransform>().rect.size;
        textGen.Populate( str, text.GetGenerationSettings( extents ) );

        int newLine = str.Substring( 0, charIndex ).Split( '\n' ).Length - 1;
        int indexOfTextQuad = ( ( charIndex ) * 4 ) + ( newLine * 4 ) - 4;
        if( indexOfTextQuad < textGen.vertexCount )
        {
            Vector3 avgPos = ( textGen.verts[indexOfTextQuad].position +
                textGen.verts[indexOfTextQuad + 1].position +
                textGen.verts[indexOfTextQuad + 2].position +
                textGen.verts[indexOfTextQuad + 3].position ) / 4f;
            avgPos.y = text.gameObject.transform.localPosition.y;
            return avgPos;
        }
        else
        {
            Debug.LogError( "GetCharacterPosition: Out of text bound" );
            return new Vector3();
        }
    }
}