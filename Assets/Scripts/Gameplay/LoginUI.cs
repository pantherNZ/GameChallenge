using UnityEngine;
using UnityEngine.UI;

public class LoginUI : BaseLevel
{
    [SerializeField] Button loginButton = null;
    [SerializeField] InputField passworldInput = null;
    [SerializeField] CanvasGroup loginDisplay = null;
    [SerializeField] CanvasGroup incorrectPasswordDisplay = null;
    [SerializeField] Button incorrectPasswordButton = null;
    readonly string password = "simple";

    private void Start()
    {
        GetComponent<CanvasGroup>().SetVisibility( false );

        loginButton.onClick.AddListener( () =>
        {
            if( passworldInput.text.Length > 0 && passworldInput.text == password )
            {
                GetComponent<CanvasGroup>().ToggleVisibility();
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
                passworldInput.text = string.Empty;
            }
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
        GetComponent<CanvasGroup>().SetVisibility( true );
        Utility.FunctionTimer.CreateTimer( 1.0f, () => SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_1_1" ) ) );
        Utility.FunctionTimer.CreateTimer( 20.0f, () => SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_1_2" ) ), "2nd_prompt" );
        Utility.FunctionTimer.CreateTimer( 35.0f, () =>
        {
            SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_1_3" ) );
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

                if( index + 1 >= password.Length )
                {
                    Utility.FunctionTimer.StopTimer( "input_password" );
                    Utility.FunctionTimer.StopTimer( "input_password_loop" );
                    Utility.FunctionTimer.CreateTimer( 0.8f, () => loginButton.onClick.Invoke() );
                }
            }, "input_password_loop", true );
        }, "input_password" );
    }
}
