using UnityEngine;
using UnityEngine.UI;

public class Level10_PassCode : BaseLevel
{
    // Static data
    [SerializeField] GameObject passCodeUI = null;
    [SerializeField] CanvasGroup canvas = null;
    [SerializeField] Text passCodeText = null;

    // Dynamic data
    int subtitleIndex = 0;

    // Functions
    private void Start()
    {
        canvas.SetVisibility( false );
    }

    public override void OnStartLevel()
    {
        PlaySubtitle();
        Utility.FunctionTimer.CreateTimer( 5.0f, PlaySubtitle, "PlaySubtitle", true );
        canvas.SetVisibility( true );
    }

    protected override void OnLevelUpdate()
    {

    }

    protected override void OnLevelFinished()
    {
        passCodeUI.Destroy();
        Utility.FunctionTimer.StopTimer( "PlaySubtitle" );
    }

    void PlaySubtitle()
    {
        ++subtitleIndex;
        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_10_" + subtitleIndex.ToString() );

        if( subtitleIndex >= 6 )
            subtitleIndex -= 3;
    }

    public void ButtonPressed( int value )
    {
        if( value == 10 )
        {

        }
        else if( value == 11 )
        {
            if( passCodeText.text.Length > 0 )
                passCodeText.text = passCodeText.text.Remove( passCodeText.text.Length - 3 );
        }
        else
        {
            passCodeText.text += value.ToString() + "  ";
        }
    }
}