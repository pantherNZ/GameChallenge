using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] LoginUI loginUI = null;
    [SerializeField] DesktopUIManager desktopUI = null;
    [SerializeField] bool startEnabled = true;

    void Start()
    {
        if( !startEnabled )
            return;

        GetComponent<CanvasGroup>().SetVisibility( true );
        loginUI.GetComponent<CanvasGroup>().SetVisibility( false );
        desktopUI.GetComponent<CanvasGroup>().SetVisibility( false );

        Utility.FunctionTimer.CreateTimer( 2.0f, () =>
        {
            loginUI.Display();
            GetComponent<CanvasGroup>().ToggleVisibility();
        } );
    }
}
