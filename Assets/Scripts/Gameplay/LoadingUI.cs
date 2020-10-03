using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingUI : BaseLevel
{
    [SerializeField] DesktopUIManager desktopUI = null;

    public override void OnStartLevel()
    {
        GetComponent<CanvasGroup>().SetVisibility( true );
        desktopUI.GetComponent<CanvasGroup>().SetVisibility( false );

        Utility.FunctionTimer.CreateTimer( 2.0f, () =>
        {
            StartNextLevel();
            GetComponent<CanvasGroup>().ToggleVisibility();
        } );
    }
}
