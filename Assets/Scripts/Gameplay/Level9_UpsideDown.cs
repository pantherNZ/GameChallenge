using System.Collections.Generic;
using System;
using UnityEngine;

public class Level9_UpsideDown : BaseLevel
{
    // Static data
    [SerializeField] RectTransform subtitlesCanvas = null;

    // Dynamic data

    // Functions
    public override void OnStartLevel()
    {
        desktop.DesktopCanvas.localRotation = Quaternion.Euler( 0.0f, 0.0f, 180.0f );
        subtitlesCanvas.localRotation = Quaternion.Euler( 0.0f, 0.0f, 180.0f );

        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_9_1" );
    }

    protected override void OnLevelFinished()
    {
        desktop.DesktopCanvas.localRotation = Quaternion.identity;
        subtitlesCanvas.localRotation = Quaternion.identity;
    }

    public override string GetSpoilerText()
    {
        return DataManager.Instance.GetGameString( "Spoiler_Level9" );
    }
}