using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] LoginUI loginUI = null;
    [SerializeField] DesktopUIManager desktopUI = null;
    [SerializeField] int forceStartLevelId = 0;

    void Start()
    {
        GetComponent<CanvasGroup>().SetVisibility( true );
        loginUI.GetComponent<CanvasGroup>().SetVisibility( false );
        desktopUI.GetComponent<CanvasGroup>().SetVisibility( false );

        if( forceStartLevelId != 0 )
        {
            GetComponent<CanvasGroup>().ToggleVisibility();
            if( forceStartLevelId == -1 )
                Utility.FunctionTimer.CreateTimer( 0.1f, desktopUI.StartLevel );
            else if( forceStartLevelId == 1 )
                Utility.FunctionTimer.CreateTimer( 0.1f, desktopUI.GetComponent<Level1_BouncingBall>().StartLevel );
            else if( forceStartLevelId == 3 )
                Utility.FunctionTimer.CreateTimer( 0.1f, desktopUI.GetComponent<Level3_Recycling>().StartLevel );
            else
                Debug.LogError( "LoadingUI: forceStartLevelId invalid value: " + forceStartLevelId.ToString() );
            return;
        }

        Utility.FunctionTimer.CreateTimer( 2.0f, () =>
        {
            loginUI.StartLevel();
            GetComponent<CanvasGroup>().ToggleVisibility();
        } );
    }
}
