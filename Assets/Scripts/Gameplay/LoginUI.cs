using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    public Button loginButton = null;
    public InputField passworldInput = null;
    public CanvasGroup loginDisplay = null;
    public CanvasGroup incorrectPasswordDisplay = null;
    public CanvasGroup desktopUI = null;
    public Button incorrectPasswordButton = null;
    readonly string password = "simple";

    private void Start()
    {
        loginButton.onClick.AddListener( () =>
        {
            if( passworldInput.text.Length > 0 && passworldInput.text == password )
            {
                GetComponent<CanvasGroup>().ToggleVisibility();
                desktopUI.ToggleVisibility();
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

    private void Update()
    {
        if( Input.GetKeyUp( KeyCode.Return ) )
        {
            loginButton.onClick.Invoke();
        }
    }

    int index;

    public void Display()
    {
        GetComponent<CanvasGroup>().ToggleVisibility();
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

                if( index >= password.Length )
                {
                    Utility.FunctionTimer.StopTimer( "input_password_loop" );
                    Utility.FunctionTimer.CreateTimer( 0.8f, () => loginButton.onClick.Invoke() );
                }
            }, "input_password_loop", true );
        }, "input_password" );
    }
}
