using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingUI : BaseLevel
{
    [SerializeField] LoginUI loginUI = null;
    [SerializeField] DesktopUIManager desktopUI = null;

    public override void OnStartLevel()
    {
        GetComponent<CanvasGroup>().SetVisibility( true );
        loginUI.GetComponent<CanvasGroup>().SetVisibility( false );
        desktopUI.GetComponent<CanvasGroup>().SetVisibility( false );

        Utility.FunctionTimer.CreateTimer( 2.0f, () =>
        {
            loginUI.StartLevel();
            GetComponent<CanvasGroup>().ToggleVisibility();
        } );
    }
}
