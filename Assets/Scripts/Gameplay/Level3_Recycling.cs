﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level3_Recycling : BaseLevel
{
    [Serializable]
    class Round
    {
        public float timerEasySec = 20.0f;
        public float timerHardSec = 15.0f;
        public int numShortcutsEasy = 10;
        public int numShortcutsHard = 25;
        public string subtitleEasy = string.Empty;
        public string subtitleHard = string.Empty;
    }

    [SerializeField] List<DesktopIcon> data = new List<DesktopIcon>();
    [SerializeField] List<Round> rounds = new List<Round>();
    [SerializeField] int roundNumber = 0;
    [SerializeField] int fixedSeed = 0;
    [SerializeField] AudioClip roundCompleteAudio = null;
    List<GameObject> shortcuts = new List<GameObject>();
    Utility.FunctionTimer timer;
    
    public override void OnStartLevel()
    {
        roundNumber = 0;
        if( fixedSeed != 0 )
            UnityEngine.Random.InitState( fixedSeed );
        GetComponent<CanvasGroup>().SetVisibility( true );
        Setup();
    }

    private void Setup()
    { 
        if( roundNumber > rounds.Count )
            return;

        Cleanup( false );
        var worldRect = ( SubtitlesManager.Instance.transform as RectTransform ).GetWorldRect();

        for( int i = 0; i < ( desktop.IsEasyMode() ? rounds[roundNumber].numShortcutsEasy : rounds[roundNumber].numShortcutsHard ); ++i )
        {
            var item = data.RandomItem();

            var shortcut = desktop.CreateShortcut( item, desktop.GetGridBounds().RandomPosition() );
            shortcuts.Add( shortcut );
            var rectTransform = ( shortcut.transform as RectTransform );
            int safety = 0;

            while( ++safety <= 300 )
            {
                rectTransform.localPosition = desktop.GetGridBounds().RandomPosition();
                rectTransform.ForceUpdateRectTransforms();
                var shortcutWorldRect = rectTransform.GetWorldRect();
                if( !worldRect.Overlaps( shortcutWorldRect ) )
                    break;
            }
        }

        var subtitle = desktop.IsEasyMode() ? rounds[roundNumber].subtitleEasy : rounds[roundNumber].subtitleHard;
        SubtitlesManager.Instance.AddSubtitleGameString( subtitle );

        var duration = desktop.IsEasyMode() ? rounds[roundNumber].timerEasySec : rounds[roundNumber].timerHardSec;
        if( duration > 0.0f )
        {
            timer = Utility.FunctionTimer.CreateTimer( duration, () =>
            {
                SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_3_Failed" );
                desktop.LevelFailed();
            }, "Level_3" );

            SubtitlesManager.Instance.AssignTimer( timer );
            timer.Pause();
        }

        if( roundNumber > 0 )
            desktop.PlayAudio( roundCompleteAudio );
    }

    protected override void OnLevelUpdate()
    {
        if( shortcuts.RemoveAll( x => x == null ) > 0 )
            timer?.Resume();

        if( desktop != null && shortcuts.Count == 0 )
        {
            ++roundNumber;

            if( roundNumber < rounds.Count )
            {
                Setup();
            }
            else
            {
                SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_3_Complete" );
                LevelFinished( 3.0f );
            }
        }
    }

    protected override void Cleanup( bool fromRestart )
    {
        for( int i = shortcuts.Count - 1; i >= 0; --i )
            desktop.RemoveShortcut( shortcuts[i] );

        shortcuts.Clear();
        Utility.FunctionTimer.StopTimer( "Level_3" );
    }
}
