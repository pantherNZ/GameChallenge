using UnityEngine;
using UnityEngine.UI;

public class LoginUI : BaseLevel
{
    [SerializeField] Button loginButton = null;
    [SerializeField] CanvasGroup hintButton = null;
    [SerializeField] CanvasGroup hintText = null;
    [SerializeField] Button flag = null;
    [SerializeField] InputField passworldInput = null;
    [SerializeField] CanvasGroup loginDisplay = null;
    [SerializeField] CanvasGroup incorrectPasswordDisplay = null;
    [SerializeField] Button incorrectPasswordButton = null;
    [SerializeField] AudioClip incorrectPasswordAudio = null;
    [SerializeField] float fadeInTime = 1.5f;
    readonly string password = "simple";

    private void Start()
    {
        GetComponent<CanvasGroup>().SetVisibility( false );

        loginButton.onClick.AddListener( () =>
        {
            if( passworldInput.text.Length > 0 && passworldInput.text == password )
            {
                GetComponent<CanvasGroup>().ToggleVisibility();
                desktop.GetComponent<CanvasGroup>().SetVisibility( true );
                LevelFinished();
                StartNextLevel();
                Utility.FunctionTimer.StopTimer( "2nd_prompt" );
                Utility.FunctionTimer.StopTimer( "3nd_prompt" );
                Utility.FunctionTimer.StopTimer( "input_password" );
                Utility.FunctionTimer.StopTimer( "input_password_loop" );
            }
            else
            {
                loginDisplay.ToggleVisibility();
                incorrectPasswordDisplay.ToggleVisibility();
                desktop.PlayAudio( incorrectPasswordAudio );
                hintButton.GetComponent<Button>().interactable = true;
                hintButton.SetVisibility( true );
                hintText.SetVisibility( false );
                if( flag != null )
                    flag.interactable = true;
            }

            passworldInput.text = string.Empty;
        } );

        incorrectPasswordButton.onClick.AddListener( () =>
        {
            loginDisplay.ToggleVisibility();
            incorrectPasswordDisplay.ToggleVisibility();
        } );
    }

    protected override void OnLevelUpdate()
    {
        if( Input.GetKeyUp( KeyCode.Return ) )
        {
            loginButton.onClick.Invoke();
        }
    }

    int index;

    public override void OnStartLevel()
    {
        passworldInput.inputType = InputField.InputType.Password;
        passworldInput.text = string.Empty;
        passworldInput.readOnly = false;

        loginDisplay.SetVisibility( true );
        GetComponent<CanvasGroup>().SetVisibility( true );
        desktop.GetComponent<CanvasGroup>().SetVisibility( false );
        hintButton.SetVisibility( false );
        hintButton.GetComponent<Button>().interactable = false;
        hintText.SetVisibility( false );
        if( flag != null )
            flag.interactable = false;

        this.FadeFromBlack( fadeInTime );
        Utility.FunctionTimer.CreateTimer( 1.0f, () => SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_0_1" ) );
        Utility.FunctionTimer.CreateTimer( 20.0f, () => SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_0_2" ), "2nd_prompt" );
        Utility.FunctionTimer.CreateTimer( 35.0f, () =>
        {
            SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_0_Complete" );
            passworldInput.inputType = InputField.InputType.Standard;
            passworldInput.text = string.Empty;
            passworldInput.readOnly = true;
        }, "3nd_prompt" );

        Utility.FunctionTimer.CreateTimer( 38.0f, () =>
        {
            Utility.FunctionTimer.CreateTimer( 0.1f, () =>
            {
                passworldInput.text = password.Substring( 0, index + 1 );
                index++;

                if( index >= password.Length )
                {
                    Utility.FunctionTimer.StopTimer( "input_password" );
                    Utility.FunctionTimer.StopTimer( "input_password_loop" );
                    Utility.FunctionTimer.CreateTimer( 0.8f, () => loginButton.onClick.Invoke() );
                }
            }, "input_password_loop", true );
        }, "input_password" );
    }
}
