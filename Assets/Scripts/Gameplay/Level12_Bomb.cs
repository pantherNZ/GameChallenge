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
    [SerializeField] Image mainImage = null;
    [SerializeField] Image progressImage = null;
    [SerializeField] Sprite[] stage1UI = new Sprite[3];
    [SerializeField] Sprite[] stage2UI = new Sprite[3];
    [SerializeField] Sprite[] stage3UI = new Sprite[3];

    [Serializable]
    public class Connections
    {
        public List<Toggle> connections = new List<Toggle>();
    }

    [SerializeField] List<Connections> buttonConnections = new List<Connections>();
    [SerializeField] InputField passwordInput = null;
    [SerializeField] List<Color> stageColours = new List<Color>();
    [SerializeField] List<int> stageTextures = new List<int>();

    [SerializeField] float progressSpeedStart = 1.0f;
    float progressSpeed = 1.0f;

    [SerializeField] float lightSequenceThresholdEasy = 0.85f;
    [SerializeField] float lightSequenceThresholdHard = 0.95f;

    // Dynamic data
    SubLevelStage subLevelStage = SubLevelStage.Intro;
    //int timeLeftSec;
    bool insideCheckboxCallback = false;
    bool progressDirection = true;
    List<int> lightSequence;
    int lightIndex = 0;
    Color originalCheckboxColour;

    enum SubLevelStage
    {
        Intro,
        Buttons,
        Buttons2,
        Buttons3,
        ButtonMash,
        ProgressBars,
        ProgressBars2,
        TimeMatch,
        Passcode,
        Finished,
    }

    int counter = 0;
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

        originalCheckboxColour = buttons[0].targetGraphic.color;
    }

    public override void OnStartLevel()
    {
        window.GetComponent<CanvasGroup>().SetVisibility( true );
        ( window.transform as RectTransform ).anchoredPosition = desktop.DesktopCanvas.anchoredPosition;
        progressBar = window.GetComponentInChildren<UIProgressBar>();
        IncrementStage();
        UpdateText();
        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_12_Start" );
    }

    private void ModifyButtonState( Action action )
    {
        insideCheckboxCallback = true;
        action();
        insideCheckboxCallback = false;
    }

    protected override void OnLevelUpdate()
    {
        if( Input.GetKeyDown( KeyCode.Space ) )
            IncrementStage();

        switch( subLevelStage )
        {
            case SubLevelStage.Buttons:
                // Fall through
            case SubLevelStage.Buttons2:
                // Fall through
            case SubLevelStage.Buttons3:
            {
                if( buttons.All( ( x ) => x.isOn ) )
                    IncrementStage();

                break;
            }
            case SubLevelStage.ButtonMash:
            {
                if( Input.anyKeyDown )
                {
                    counter++;
                    UpdateText();

                    var max = desktop.IsEasyMode() ? maxKeysEasy : maxKeysHard;
                    var threshold = max / 6;

                    if( ( counter % threshold ) == 0 )
                        buttons[lightSequence[counter / threshold - 1]].isOn = true;

                    if( counter >= max )
                        IncrementStage();
                }

                break;
            }
            case SubLevelStage.ProgressBars:
            case SubLevelStage.ProgressBars2:
            {
                if( buttons.All( ( x ) => x.isOn ) )
                {
                    IncrementStage();
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
                        buttons[lightSequence[lightIndex]].targetGraphic.color = stageColours[( int )subLevelStage];
                    else if( oldPogress >= threshold && progressBar.Progress < threshold && !buttons[lightSequence[lightIndex]].isOn )
                        buttons[lightSequence[lightIndex]].targetGraphic.color = originalCheckboxColour;
                }

                break;
            }

            case SubLevelStage.TimeMatch:
            {
                if( Mathf.Abs( ( float )( desktop.GetVirtualTime().TotalSeconds - DateTime.Now.TimeOfDay.TotalSeconds ) ) < 1.0f )
                    IncrementStage();

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
            //SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_12_Finish" );
            Utility.FunctionTimer.CreateTimer( 3.0f, desktop.FinishGame );
            window.GetComponent<CanvasGroup>().SetVisibility( false );
            hackingCanvas.SetVisibility( false );
            return;
        }

        hackingCanvas.SetVisibility( false );
        var stageUI = stageTextures[( int )subLevelStage] == 1 ? stage1UI : stageTextures[( int )subLevelStage] == 2 ? stage2UI : stage3UI;
        mainImage.sprite = stageUI[0];
        mainImage.color = stageColours[( int )subLevelStage];
        progressImage.sprite = stageUI[1];
        progressImage.color = stageColours[( int )subLevelStage];

        foreach( var button in buttons )
        {
            button.graphic.GetComponent<Image>().sprite = stageUI[2];
            button.graphic.color = stageColours[( int )subLevelStage];
            ModifyButtonState( () => button.isOn = false );
            button.onValueChanged.RemoveAllListeners();        
            button.enabled = true;
            button.targetGraphic.color = originalCheckboxColour;
        }

        lightSequence = lightSequence.RandomShuffle();
        progressSpeed = progressSpeedStart;
        progressBar.Progress = 0.0f;
        lightIndex = 0;

        window.GetComponentInChildren<Draggable>().enabled = false;
        SetupNewStage();
    }

    private void SetupNewStage()
    {
        switch( subLevelStage )
        {
            case SubLevelStage.Buttons:
                break;

            case SubLevelStage.Buttons2:
            {
                for( int i = 0; i < buttonConnections.Count; ++i )
                {
                    int index = i;
                    buttons[index].onValueChanged.AddListener( ( x ) =>
                    {
                        if( insideCheckboxCallback )
                            return;

                        if( !x )
                        {
                            ModifyButtonState( () => buttons[index].isOn = true );
                            return;
                        }

                        if( lightSequence[lightIndex] != index )
                        {
                            ModifyButtonState( () =>
                            {
                                foreach( var b in buttons )
                                    b.isOn = false;
                            } );
                            lightIndex = 0;
                        }
                        else
                            lightIndex++;
                    } );
                }
            }
            break;

            case SubLevelStage.Buttons3:
            {
                for( int i = 0; i < buttonConnections.Count; ++i )
                {
                    int index = i;
                    buttons[index].onValueChanged.AddListener( ( x ) =>
                    {
                        if( insideCheckboxCallback )
                            return;

                        ModifyButtonState( () =>
                        {
                            buttons[index].isOn = !x;
                            foreach( var connection in buttonConnections[index].connections )
                                connection.isOn = !connection.isOn;
                        } );
                    } );
                }
            }
            break;

            case SubLevelStage.ButtonMash:
            {
                hackingCanvas.SetVisibility( true );
                hackingCanvas.GetComponentInChildren<Text>().GetComponent<CanvasGroup>().SetVisibility( true );
                SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_12_Hack" );
                foreach( var button in buttons )
                    button.enabled = false;
            }
            break;

            case SubLevelStage.ProgressBars:
            {
                for( int i = 0; i < buttons.Count; ++i )
                    lightSequence[i] = i;

                goto case SubLevelStage.ProgressBars2;
            }
            case SubLevelStage.ProgressBars2:
            {
                hackingCanvas.SetVisibility( true );
                hackingCanvas.GetComponentInChildren<Text>().GetComponent<CanvasGroup>().SetVisibility( false );

                foreach( var button in buttons )
                {
                    button.targetGraphic.color = originalCheckboxColour;
                    button.onValueChanged.AddListener( ( x ) =>
                    {
                        if( insideCheckboxCallback )
                            return;

                        if( x )
                        {
                            if( button.targetGraphic.color == originalCheckboxColour )
                            {
                                ModifyButtonState( () =>
                                {
                                    foreach( var b in buttons )
                                    {
                                        b.isOn = false;
                                        b.targetGraphic.color = originalCheckboxColour;
                                    }
                                } );
                                progressSpeed = progressSpeedStart;

                            }
                            else
                                progressSpeed += 0.08f;
                        }
                    } );
                }
            }
            break;

            case SubLevelStage.TimeMatch:
            {
                if( Mathf.Abs( ( float )( desktop.GetVirtualTime().TotalSeconds - DateTime.Now.TimeOfDay.TotalSeconds ) ) < 1.0f )
                    IncrementStage();
                else
                    SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_12_Time1" );
            }
            break;

            case SubLevelStage.Passcode:
            {
                passwordInput.interactable = true;
                var passwordBtn = passwordInput.GetComponentInChildren<Button>();
                passwordBtn.interactable = true;
                passwordBtn.onClick.AddListener( () =>
                {
                    if( passwordInput.text == "Alex" )
                    {
                        IncrementStage();
                    }
                    else
                    {
                        passwordInput.text = string.Empty;
                        // Play fail audio
                    }
                } );

                hackingCanvas.SetVisibility( true );
                hackingCanvas.GetComponentInChildren<Text>().GetComponent<CanvasGroup>().SetVisibility( false );
                window.GetComponentInChildren<Draggable>().enabled = true;
                SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_12_MoveWindow" );
                foreach( var b in buttons )
                    b.enabled = false;
            }
            break;
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