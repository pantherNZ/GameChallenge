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

    [Serializable]
    public class Connections
    {
        public List<Toggle> connections = new List<Toggle>();
    }

    [SerializeField] List<Connections> buttonConnections = new List<Connections>();
    [SerializeField] int maxTimeSec = 300;
    [SerializeField] Text timerText = null;
    [SerializeField] List<Color> stageColours;

    // Dynamic data
    SubLevelStage subLevelStage = SubLevelStage.Buttons;
    int timeLeftSec;
    bool insideCheckboxCallback = false;

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

        stageColours = new List<Color>()
        {
            Color.red,
            Color.green,
            Color.blue,
        };
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
                    IncrementStage();

                    //Utility.FunctionTimer.CreateTimer( 0.5f, () =>
                    //{
                        insideCheckboxCallback = true;
                        foreach( var x in buttons )
                            x.isOn = false;
                        insideCheckboxCallback = false;
                   // } );

                    for( int i = 0; i < buttonConnections.Count; ++i )
                    {
                        int index = i;
                        buttons[index].onValueChanged.AddListener( ( x ) =>
                        {
                            if( !insideCheckboxCallback )
                            {
                                insideCheckboxCallback = true;
                                buttons[index].isOn = !x;
                                foreach( var connection in buttonConnections[index].connections )
                                    connection.isOn = !connection.isOn;
                                insideCheckboxCallback = false;
                            }
                        } );
                    }             
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

    private void IncrementStage()
    {
        subLevelStage++;
        foreach( var button in buttons )
            button.colors = new ColorBlock()
            {
                normalColor = stageColours[( int )subLevelStage],
                highlightedColor = stageColours[( int )subLevelStage],
                selectedColor = stageColours[( int )subLevelStage],
                pressedColor = stageColours[( int )subLevelStage].SetA( stageColours[( int )subLevelStage].a - 0.1f ),
                colorMultiplier = 1.0f,
           };
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