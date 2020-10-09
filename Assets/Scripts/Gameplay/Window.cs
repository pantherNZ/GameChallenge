using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Window : MonoBehaviour
{
    public Camera windowCamera;
    public GameObject image = null;
    [SerializeField] Text titleText = null;
    [SerializeField] Button closeButton = null;
    RenderTexture renderTexture;

    public void Initialise( string title, DesktopUIManager desktop, Camera camera, RenderTexture rt )
    {
        titleText.text = title;
        windowCamera = camera;
        renderTexture = rt;
        windowCamera.targetTexture = rt;
        closeButton.onClick.AddListener( () => { desktop.DestroyWindow( this ); } );
        image.GetComponent<RawImage>().texture = rt;
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