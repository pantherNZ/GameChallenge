using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubtitlesManager : MonoBehaviour
{
    [HideInInspector] public static SubtitlesManager Instance { get; private set; }
    string currentText = string.Empty;
    int index;
    float timer;
    float fadeOutTime, fadeOutDelay;
    public Text text = null;
    public bool appearInstantly;
    public float updateIntervalSec = 0.05f;

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

    public void AddSubtitle( string subtitle, float fadeOutDelay = 0.0f, float fadeOutTime = 0.0f )
    {
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
        }
    }

    public void ClearSubtitles()
    {
        text.text = currentText = string.Empty;
        canvasGroup.SetVisibility( false );
    }

}