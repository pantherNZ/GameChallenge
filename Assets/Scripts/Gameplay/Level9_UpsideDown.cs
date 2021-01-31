using System.Collections.Generic;
using System;
using UnityEngine;

public class Level9_UpsideDown : BaseLevel
{
    // Static data
    [SerializeField] RectTransform subtitlesCanvas = null;
    [SerializeField] UITimeSetter timeSetter = null;
    TimeSpan startTime;

    // Dynamic data

    // Functions
    public override void OnStartLevel()
    {
        desktop.DesktopCanvas.localRotation = Quaternion.Euler( 0.0f, 0.0f, 180.0f );
        subtitlesCanvas.localRotation = Quaternion.Euler( 0.0f, 0.0f, 180.0f );
        startTime = DateTime.Now.TimeOfDay;

        timeSetter.OnTimeChangedEvent += ( _, oldT, newT ) => 
        {
            if( ( newT.TotalHours - startTime.TotalHours ) >= 2.5f )
                LevelFinished();
        };

        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_9_1" );
    }

    protected override void OnLevelFinished()
    {
        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_9_Complete" );
        Utility.FunctionTimer.CreateTimer( 3.0f, StartNextLevel );
    }

    protected override void Cleanup( bool fromRestart )
    {
        base.Cleanup( fromRestart );

        Utility.FunctionTimer.CreateTimer( fromRestart ? desktop.restartGameFadeOutTime : 0.0f, () =>
        {
            desktop.DesktopCanvas.localRotation = Quaternion.identity;
            subtitlesCanvas.localRotation = Quaternion.identity;
        } );
    }
}