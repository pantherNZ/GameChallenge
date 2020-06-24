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
    List<Selection> selections = new List<Selection>();

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
                {
                    timer = 0.0f;
                }
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
    }

    public struct Selection
    {
        public int index;
        public string first, second;
    }

    public void AddSubtitle( string subtitle, float fadeOutDelay = 0.0f, float fadeOutTime = 0.0f )
    {
        selectionGroup.SetActive( false );
        selections.Clear();
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
                selectionGroup.SetActive( true );
                ( selectionGroup.transform as RectTransform ).localPosition = GetCharacterPosition( text, selections.Front().index - 1 + selectionMargin + Mathf.Max( selections.Front().first.Length, selections.Front().second.Length ) / 2 );
                var buttons = selectionGroup.GetComponentsInChildren<Button>();
                buttons[0].GetComponentInChildren<Text>().text = selections.Front().first;
                buttons[1].GetComponentInChildren<Text>().text = selections.Front().second;
             }
        }
    }

    public void ClearSubtitles()
    {
        text.text = currentText = string.Empty;
        canvasGroup.SetVisibility( false );
        selectionGroup.SetActive( false );
        selections.Clear();
    }

    void CheckForSelections( ref string subtitle )
    {
        int index = 0;

        while( index != -1 && index < subtitle.Length )
        {
            var keyword = "<select=";
            index = subtitle.IndexOf( keyword, index );
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
                    second = subtitle.Substring( end + 1, index - end - 1 )
                } );

                subtitle = subtitle.Remove( start, index - start + 1 );
                subtitle = subtitle.Insert( start, new string( ' ', Mathf.Max( selections.Back().first.Length, selections.Back().second.Length ) + selectionMargin * 2 ) );
            }
        }
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