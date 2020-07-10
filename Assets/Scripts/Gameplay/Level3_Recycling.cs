using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level3_Recycling : MonoBehaviour
{
    [Serializable]
    class Round
    {
        public float timerEasySec = 20.0f;
        public float timerHardSec = 15.0f;
        public int numShortcutsEasy = 10;
        public int numShortcutsHard = 25;
    }

    [Serializable]
    class Data : Pair<string, Texture2D>{ }

    [SerializeField] List<Data> data = new List<Data>();
    [SerializeField] List<Round> rounds = new List<Round>();
    [SerializeField] int roundNumber = 0;
    List<GameObject> shortcuts = new List<GameObject>();
    DesktopUIManager desktop;

    public void StartLevel()
    {
        desktop = GetComponent<DesktopUIManager>();
        GetComponent<CanvasGroup>().SetVisibility( true );

        if( roundNumber >= rounds.Count )
            return;

        for( int i = 0; i < ( desktop.IsEasyMode() ? rounds[roundNumber].numShortcutsEasy : rounds[roundNumber].numShortcutsHard ); ++i )
        {
            var item = data.RandomItem();
            shortcuts.Add( desktop.CreateShortcut( item.First, item.Second, desktop.GetGridBounds().RandomPosition() ) );
        }

        SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_3_1" ) );

        var timer = Utility.FunctionTimer.CreateTimer( desktop.IsEasyMode() ? rounds[roundNumber].timerEasySec : rounds[roundNumber].timerHardSec, () =>
        {
            SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_3_Failed" ) );

            for( int i = desktop.shortcuts.Count - 1; i < 0; --i )
            {
                desktop.shortcuts[i].Destroy();
                desktop.shortcuts.RemoveAt( i );
            }

            enabled = false;

            Utility.FunctionTimer.CreateTimer( 3.0f, () => desktop.GameOver() );

        }, "Level_3" );

        SubtitlesManager.Instance.AssignTimer( timer );
        SubtitlesManager.Instance.QueueSubtitleGameString( desktop.IsEasyMode() ? rounds[roundNumber].timerEasySec : rounds[roundNumber].timerHardSec, "Narrator_Level_3_Failed" );
    }

    private void Update()
    {
        if( desktop != null && desktop.shortcuts.Count == 1 )
        {
            enabled = false;
            Utility.FunctionTimer.StopTimer( "Level_3" );
            SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_3_Complete" ) );
        }
    }
}
