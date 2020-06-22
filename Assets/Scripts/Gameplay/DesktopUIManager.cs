using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class DesktopUIManager : MonoBehaviour
{
    [SerializeField] GameObject windowCamera = null;
    public GameObject WindowCamera { get => windowCamera; private set { } }

    [SerializeField] GameObject windowBase = null;
    [SerializeField] CanvasGroup startMenu = null;
    [SerializeField] Button startMenuButton = null;
    [SerializeField] Text timeDateText = null;
    System.DateTime currentTime;
    GameObject currentPointerTarget;

    // Selection box
    [SerializeField] GameObject selectionBoxPrefab = null;
    GameObject selectionBox;
    Vector3? selectionStartPos;

    public void Display()
    {
        GetComponent<CanvasGroup>().SetVisibility( true );
    }

    public void CloseGame()
    {
        // Save
        Application.Quit();
    }

    public GameObject CreateWindow()
    {
        return Instantiate( windowBase, transform );
    }

    void Start()
    {
        startMenuButton.onClick.AddListener( () => { startMenu.ToggleVisibility(); } );
    }

    void Update()
    {
        if( Input.GetMouseButtonDown( 0 ) && selectionStartPos == null )
        {
            var pointerData = new PointerEventData( EventSystem.current ) { pointerId = -1, position = Input.mousePosition };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll( pointerData, results );
            var pointerTarget = results.IsEmpty() ? null : results.Front().gameObject;

            if( startMenu.IsVisible() )
            {
                while( pointerTarget != null && pointerTarget.transform.parent != null && pointerTarget != startMenu.gameObject && pointerTarget != startMenuButton.gameObject )
                    pointerTarget = pointerTarget.transform.parent.gameObject;

                if( results.IsEmpty() || ( pointerTarget != startMenu.gameObject && pointerTarget != startMenuButton.gameObject ) )
                    startMenu.ToggleVisibility();
            }

            // Start selection box
            if( pointerTarget == null )
                selectionStartPos = Input.mousePosition;
        }

        // End selection box
        if( Input.GetMouseButtonUp( 0 ) )
        {
            selectionStartPos = null;
            if( selectionBox != null )
                selectionBox.Destroy();
        }

        if( selectionStartPos != null && Input.mousePosition != selectionStartPos )
        {
            if( selectionBox == null )
            {
                selectionBox = Instantiate( selectionBoxPrefab, transform );
                selectionBox.transform.SetAsFirstSibling();
            }

            var difference = Input.mousePosition - selectionStartPos.Value;
            ( selectionBox.transform as RectTransform ).anchoredPosition = ( Input.mousePosition + selectionStartPos.Value ) / 2.0f;
            ( selectionBox.transform as RectTransform ).sizeDelta = new Vector2( Mathf.Abs( difference.x ), Mathf.Abs( difference.y ) );
        }

        // Time update on taskbar
        var newTime = System.DateTime.Now;

        if( newTime != currentTime )
        {
            currentTime = newTime;
            timeDateText.text = newTime.ToString( "h:mm tt\nM/dd/yyyy", System.Globalization.CultureInfo.CreateSpecificCulture( "en-US" ) );
        }
    }
}
