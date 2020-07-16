using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Window : MonoBehaviour
{
    new Camera camera;
    public GameObject image = null;
    [SerializeField] Text titleText = null;
    [SerializeField] Button closeButton = null;

    void Start()
    {
        camera = Camera.main;
    }

    public void Initialise( string title, DesktopUIManager desktop )
    {
        titleText.text = title;
        closeButton.onClick.AddListener( () => { desktop.DestroyWindow( this ); } );
    }

    public string GetTitle()
    {
        return titleText.text;
    }

    public void GetCameraViewWorldCorners( Vector3[] corners )
    {
        Debug.Assert( HasViewPort() );
        if( HasViewPort() )
            ( image.transform as RectTransform ).GetWorldCorners( corners );
    }

    public bool HasViewPort()
    {
        return image != null;
    }
}