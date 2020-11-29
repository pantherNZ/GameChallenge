using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Level12_Bomb : BaseLevel
{
    // Static data
    [SerializeField] GameObject window = null;
    [SerializeField] Text buttonMashText = null;
    [SerializeField] int maxKeysEasy = 256;
    [SerializeField] int maxKeysHard = 512;
    [SerializeField] List<Toggle> buttons = new List<Toggle>();
    [SerializeField] int maxTimeSec = 300;
    [SerializeField] Text timerText = null;

    // Dynamic data
    SubLevelStage subLevelStage = SubLevelStage.Buttons;
    int timeLeftSec;

    enum SubLevelStage
    {
        Buttons,
        Buttons2,
        ButtonMash,
        Icons,
    }

    int counter = 0;
    UIProgressBar progressBar;

    // Functions
    private void Start()
    {
        window.GetComponent<CanvasGroup>().SetVisibility( false );
        timeLeftSec = maxTimeSec;
        timerText.text = TimeSpan.FromSeconds( timeLeftSec ).ToString( @"mm\:ss" );
    }

    public override void OnStartLevel()
    {
        window.GetComponent<CanvasGroup>().SetVisibility( true );
        ( window.transform as RectTransform ).anchoredPosition = desktop.DesktopCanvas.anchoredPosition;
        progressBar = window.GetComponentInChildren<UIProgressBar>();
        UpdateText();
    }

    protected override void OnLevelUpdate()
    {
        switch( subLevelStage )
        {
            case SubLevelStage.Buttons:
            {
                if( buttons.All( ( x ) => x.isOn ) )
                {
                    StartCoroutine( RunTimer() );
                    subLevelStage = SubLevelStage.Buttons2;

                    Utility.FunctionTimer.CreateTimer( 0.5f, () =>
                    {
                        foreach( var x in buttons )
                            x.isOn = false;
                    } );

                    buttons[0].onValueChanged.AddListener( ( x ) =>
                    {
                        if( x )
                        {
                            buttons[0].isOn = false;
                            buttons[1].isOn = true;
                            buttons[2].isOn = true;
                        }
                    } );
                }

                break;
            }
            case SubLevelStage.Buttons2:
            {
                if( buttons.All( ( x ) => x.isOn ) )
                {
                    
                }

                break;
            }
            case SubLevelStage.ButtonMash:
            {
                if( Input.anyKeyDown )
                {
                    counter++;
                    UpdateText();
                }

                break;
            }
                
            case SubLevelStage.Icons:
                break;
        }
    }

    private IEnumerator RunTimer()
    {
        while( timeLeftSec > 0 )
        {
            timeLeftSec--;
            timerText.text = TimeSpan.FromSeconds( timeLeftSec ).ToString( @"mm\:ss" );
            yield return new WaitForSeconds( 1.0f );
        }
    }

    private void UpdateText()
    {
        var total = desktop.IsEasyMode() ? maxKeysEasy : maxKeysHard;
        progressBar.Progress = counter / ( float )total;
        buttonMashText.text = DataManager.Instance.GetGameStringFormatted( "Level12_Mash_Text", new object[] { counter, total } );
    }

    protected override void OnLevelFinished()
    {
        window.GetComponent<CanvasGroup>().SetVisibility( false );
    }
}