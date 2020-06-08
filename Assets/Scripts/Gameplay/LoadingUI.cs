using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingUI : MonoBehaviour
{
    public LoginUI loginUI;
    public CanvasGroup desktopUI;

    void Start()
    {
        Utility.FunctionTimer.CreateTimer( 2.0f, () =>
        {
            loginUI.Display();
            GetComponent<CanvasGroup>().ToggleVisibility();
        } );
    }
}
