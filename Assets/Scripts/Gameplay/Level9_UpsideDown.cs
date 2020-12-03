using System.Collections.Generic;
using System;
using UnityEngine;

public class Level9_UpsideDown : BaseLevel
{
    // Static data
    [SerializeField] RectTransform subtitlesCanvas = null;
    [SerializeField] UITimeSetter timeSetter = null;
    TimeSpan timeGap;
    TimeSpan currentTime;

    // Dynamic data

    // Functions
    public override void OnStartLevel()
    {
        desktop.DesktopCanvas.localRotation = Quaternion.Euler( 0.0f, 0.0f, 180.0f );
        subtitlesCanvas.localRotation = Quaternion.Euler( 0.0f, 0.0f, 180.0f );
        timeGap = TimeSpan.FromMinutes( 37 );

        timeSetter.OnTimeChangedEvent += ( _, oldT, newT ) => 
        {
            var targetTime = DateTime.Now.TimeOfDay.Add( timeGap );
            var rotation = ( ( timeSetter.Time - targetTime ).Minutes / ( float )timeGap.Minutes ) * 180.0f;
            desktop.DesktopCanvas.localRotation = Quaternion.Euler( 0.0f, 0.0f, rotation );
            subtitlesCanvas.localRotation = Quaternion.Euler( 0.0f, 0.0f, rotation );
        };

        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_9_1" );
    }

    private void Update()
    {
        var newTime = DateTime.Now.TimeOfDay;

        if( newTime.Minutes != currentTime.Minutes )
        {
            var oldTime = currentTime;
            currentTime = newTime;
            var modifiedTime = timeSetter.Time;
            timeSetter.Time = currentTime + ( modifiedTime - oldTime );
        }
    }

    protected override void OnLevelFinished()
    {
        desktop.DesktopCanvas.localRotation = Quaternion.identity;
        subtitlesCanvas.localRotation = Quaternion.identity;
    }
}