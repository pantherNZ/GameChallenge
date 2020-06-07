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
    string correctPasswordHash = "$HASH|V1$10000$r53uOrQ3a+oqPR+jGmdpDxnP+vJflbIresLp0ji7uBmRrp9/";

    private void Start()
    {
        loginButton.onClick.AddListener( () =>
        {
            if( passworldInput.text.Length > 0 && SecurePasswordHasher.Verify( passworldInput.text, correctPasswordHash ) )
            {
                GetComponent<CanvasGroup>().ToggleVisibility();
                desktopUI.ToggleVisibility();
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
}
