using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingUI : MonoBehaviour
{
    public CanvasGroup loginUI;
    public CanvasGroup desktopUI;

    void Start()
    {
        Utility.FunctionTimer.CreateTimer( 2.0f, () =>
        {
            loginUI.ToggleVisibility();
            GetComponent<CanvasGroup>().ToggleVisibility();
        } );
    }
}
