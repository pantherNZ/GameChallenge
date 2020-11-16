using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
        Utility.FunctionTimer.CreateTimer( 10.0f, PlaySubtitle, "PlaySubtitle", true );
        canvas.SetVisibility( true );
    }

    protected override void OnLevelUpdate()
    {

    }

    protected override void OnLevelFinished()
    {
        passCodeUI.Destroy();
        StartNextLevel();
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
            var codes = new string[] { "4122", "1224", "1224", "2412" };
            if( codes.Contains( passCodeText.text.Replace( " ", string.Empty ) ) )
            {
                canvas.SetVisibility( false );
                Utility.FunctionTimer.StopTimer( "PlaySubtitle" );
                Utility.FunctionTimer.CreateTimer( 1.0f, LevelFinished );
            }
            else
            {
                this.ShakeTarget( passCodeUI.transform, 0.3f, 5.0f, 3.0f, 40.0f, 2.0f );
            }
        }
        else if( value == 11 )
        {
            if( passCodeText.text.Length > 0 )
                passCodeText.text = passCodeText.text.Remove( passCodeText.text.Length - 3 );
        }
        else if( passCodeText.text.Length < 12 )
        {
            passCodeText.text += value.ToString() + "  ";
        }
    }
}