using System;
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
    }

    [SerializeField] List<DesktopIcon> data = new List<DesktopIcon>();
    [SerializeField] List<Round> rounds = new List<Round>();
    [SerializeField] int roundNumber = 0;
    [SerializeField] int fixedSeed = 0;
    List<GameObject> shortcuts = new List<GameObject>();

    public override void OnStartLevel()
    {
        if( fixedSeed != 0 )
            UnityEngine.Random.InitState( fixedSeed );
        GetComponent<CanvasGroup>().SetVisibility( true );

        if( roundNumber >= rounds.Count )
            return;

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

        SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_3_1" ) );

        var timer = Utility.FunctionTimer.CreateTimer( desktop.IsEasyMode() ? rounds[roundNumber].timerEasySec : rounds[roundNumber].timerHardSec, () =>
        {
            SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_3_Failed" ) );

            for( int i = shortcuts.Count - 1; i < 0; --i )
                desktop.RemoveShortcut( shortcuts[i] );

            shortcuts.Clear();
            enabled = false;

            desktop.LevelFailed( this );
        }, "Level_3" );

        SubtitlesManager.Instance.AssignTimer( timer );
    }

    protected override void OnLevelUpdate()
    {
        shortcuts.RemoveAll( x => x == null );

        if( desktop != null && shortcuts.Count == 0 )
        {
            enabled = false;
            Utility.FunctionTimer.StopTimer( "Level_3" );
            SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_3_Complete" ) );
            Utility.FunctionTimer.CreateTimer( 3.0f, StartNextLevel );
        }
    }
}
