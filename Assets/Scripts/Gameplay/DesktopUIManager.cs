using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class DesktopUIManager : MonoBehaviour
{
    public CanvasGroup startMenu;
    public Button startMenuButton;
    public Text timeDateText;
    GameObject currentPointerTarget;

    // Start is called before the first frame update
    void Start()
    {
        startMenuButton.onClick.AddListener( () => { startMenu.ToggleVisibility(); } );
    }

    // Update is called once per frame
    void Update()
    {
        if( Input.GetMouseButtonDown( 0 ) && startMenu.IsVisible() )
        {
            var pointerData = new PointerEventData( EventSystem.current ){ pointerId = -1, position = Input.mousePosition };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll( pointerData, results );
            var pointerTarget = results.IsEmpty() ? null : results.Front().gameObject;

            while( pointerTarget != null && pointerTarget.transform.parent != null && pointerTarget != startMenu.gameObject && pointerTarget != startMenuButton.gameObject )
                pointerTarget = pointerTarget.transform.parent.gameObject;
            
            if( results.IsEmpty() || ( pointerTarget != startMenu.gameObject && pointerTarget != startMenuButton.gameObject ) )
                startMenu.ToggleVisibility();
        }

        timeDateText.text = System.DateTime.Now.ToString( "h:mm tt\nM/dd/yyyy", System.Globalization.CultureInfo.CreateSpecificCulture( "en-US" ) );
    }
}
