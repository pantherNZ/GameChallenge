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
    [SerializeField] CanvasGroup hackingCanvas = null;

    [Serializable]
    public class Connections
    {
        public List<Toggle> connections = new List<Toggle>();
    }

    [SerializeField] List<Connections> buttonConnections = new List<Connections>();
    [SerializeField] InputField passwordInput = null;
    [SerializeField] List<Color> stageColours = new List<Color>();

    [SerializeField] float progressSpeedStart = 1.0f;
    float progressSpeed = 1.0f;

    [SerializeField] float lightSequenceThresholdEasy = 0.85f;
    [SerializeField] float lightSequenceThresholdHard = 0.95f;

    // Dynamic data
    SubLevelStage subLevelStage = SubLevelStage.Buttons;
    //int timeLeftSec;
    bool insideCheckboxCallback = false;
    bool progressDirection = true;
    List<int> lightSequence;
    int lightIndex = 0;

    enum SubLevelStage
    {
        Buttons,
        Buttons2,
        Buttons3,
        ButtonMash,
        ProgressBars,
        ProgressBars2,
        Passcode,
        Finished,
    }

    int counter = 255;
    //int counter = 0;
    UIProgressBar progressBar;

    // Functions
    private void Start()
    {
        window.GetComponent<CanvasGroup>().SetVisibility( false );
        //timeLeftSec = maxTimeSec;
        //timerText.text = TimeSpan.FromSeconds( timeLeftSec ).ToString( @"mm\:ss" );
        hackingCanvas.SetVisibility( false );

        lightSequence = new List<int>();

        for( int i = 0; i < buttons.Count; ++i )
            lightSequence.Add( i );

        progressSpeed = progressSpeedStart;
    }

    public override void OnStartLevel()
    {
        window.GetComponent<CanvasGroup>().SetVisibility( true );
        ( window.transform as RectTransform ).anchoredPosition = desktop.DesktopCanvas.anchoredPosition;
        progressBar = window.GetComponentInChildren<UIProgressBar>();
        UpdateText();
        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_12_1" );
    }

    protected override void OnLevelUpdate()
    {
        switch( subLevelStage )
        {
            case SubLevelStage.Buttons:
            {
                if( buttons.All( ( x ) => x.isOn ) )
                {
                    IncrementStage();

                    for( int i = 0; i < buttonConnections.Count; ++i )
                    {
                        int index = i;
                        buttons[index].onValueChanged.AddListener( ( x ) =>
                        {
                            if( !x )
                                return;

                            if( lightSequence[lightIndex] != index )
                            {
                                foreach( var b in buttons )
                                    b.isOn = false;
                                lightIndex = 0;
                            }
                            else
                                lightIndex++;
                        } );
                    }
                }

                break;
            }
            case SubLevelStage.Buttons2:
            {
                if( buttons.All( ( x ) => x.isOn ) )
                {
                    hackingCanvas.SetVisibility( true );
                    IncrementStage();

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
            case SubLevelStage.Buttons3:
            {
                if( buttons.All( ( x ) => x.isOn ) )
                {
                    hackingCanvas.SetVisibility( true );
                    IncrementStage();
                    SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_12_2" );
                    foreach( var button in buttons )
                        button.enabled = false;
                }

                break;
            }
            case SubLevelStage.ButtonMash:
            {
                if( Input.anyKeyDown )
                {
                    counter++;
                    UpdateText();

                    if( counter >= ( desktop.IsEasyMode() ? maxKeysEasy : maxKeysHard ) )
                    {
                        hackingCanvas.GetComponentInChildren<Text>().GetComponent<CanvasGroup>().SetVisibility( false );
                        IncrementStage();
                        progressBar.Progress = 0.0f;

                        for( int i = 0; i < buttons.Count; ++i )
                            lightSequence[i] = i;

                        foreach( var button in buttons )
                        {
                            SetButtonColour( button, Color.white );
                            button.onValueChanged.AddListener( ( x ) =>
                            {
                                if( x )
                                {
                                    if( button.colors.normalColor == Color.white )
                                    {
                                        foreach( var b in buttons )
                                        {
                                            b.isOn = false;
                                            SetButtonColour( b, Color.white );
                                        }
                                        progressSpeed = progressSpeedStart;
  
                                    }
                                    else
                                        progressSpeed += 0.08f;
                                }
                            } );
                        }
                    }
                }

                break;
            }
            case SubLevelStage.ProgressBars:
            case SubLevelStage.ProgressBars2:
            {
                if( buttons.All( ( x ) => x.isOn ) )
                {
                    IncrementStage();

                    if( subLevelStage == SubLevelStage.ProgressBars )
                    {
                        foreach( var b in buttons )
                        {
                            b.isOn = false;
                            SetButtonColour( b, Color.white );
                        }

                        hackingCanvas.SetVisibility( false );
                        passwordInput.interactable = true;
                        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_12_3" );
                    }
                    else
                    {
                        foreach( var b in buttons )
                            b.enabled = false;
                    }
                }
                else
                {
                    var oldPogress = progressBar.Progress;
                    progressBar.Progress = Mathf.Clamp01( progressBar.Progress + Time.deltaTime * ( progressDirection ? 1.0f : -1.0f ) * progressSpeed );

                    // switch directions
                    if( ( progressBar.Progress >= 1.0f && progressDirection ) || ( progressBar.Progress <= 0.0f && !progressDirection ) )
                    {
                        progressDirection = !progressDirection;
                        if( progressDirection )
                        {
                            do
                            {
                                lightIndex = ( lightIndex + 1 ) % lightSequence.Count;
                            } while( buttons[lightSequence[lightIndex]].isOn );
                        }
                    }

                    var threshold = desktop.IsEasyMode() ? lightSequenceThresholdEasy : lightSequenceThresholdHard;
                    if( oldPogress < threshold && progressBar.Progress >= threshold )
                        SetButtonColour( buttons[lightSequence[lightIndex]], stageColours[( int )subLevelStage] );
                    else if( oldPogress >= threshold && progressBar.Progress < threshold && !buttons[lightSequence[lightIndex]].isOn )
                        SetButtonColour( buttons[lightSequence[lightIndex]], Color.white );
                }

                break;
            }
                
            case SubLevelStage.Passcode:
                break;
        }
    }

    private void IncrementStage()
    {
        subLevelStage++;

        if( subLevelStage == SubLevelStage.Finished )
        {
            SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_12_Finish" );
            Utility.FunctionTimer.CreateTimer( 3.0f, desktop.FinishGame );
            return;
        }

        foreach( var button in buttons )
        {
            SetButtonColour( button, ( int )subLevelStage >= stageColours.Count ? Color.white : stageColours[( int )subLevelStage] );
            insideCheckboxCallback = true;
            button.onValueChanged.RemoveAllListeners();
            button.isOn = false;
            button.enabled = true;
            insideCheckboxCallback = false;
        }

        lightSequence = lightSequence.RandomShuffle();
        progressSpeed = progressSpeedStart;
        lightIndex = 0;
    }

    private void SetButtonColour( Toggle button, Color colour )
    {
        button.colors = new ColorBlock()
        {
            normalColor = colour,
            highlightedColor = colour,
            selectedColor = colour,
            pressedColor = colour.SetA( colour.a - 0.1f ),
            colorMultiplier = 1.0f,
        };
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