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
    DesktopUIManager desktopRef;

    public void Initialise( string title, DesktopUIManager desktop, Camera camera, RenderTexture rt )
    {
        titleText.text = title;
        windowCamera = camera;
        renderTexture = rt;
        windowCamera.targetTexture = rt;
        closeButton.onClick.AddListener( () => { desktop.DestroyWindow( this ); } );
        image.GetComponent<RawImage>().texture = rt;
        desktopRef = desktop;
    }

    public string GetTitle()
    {
        return titleText.text;
    }

    public Rect GetCameraViewWorldRect()
    {
        Debug.Assert( HasViewPort() );
        Vector3[] corners = new Vector3[4];
        ( image.transform as RectTransform ).GetWorldCorners( corners );
        return new Rect(
            desktopRef.windowCameraStartPosition + corners[0],
            new Vector2( corners[3].x - corners[0].x, corners[1].y - corners[0].y ) );
    }

    public bool HasViewPort()
    {
        return image != null;
    }

    private void Update()
    {
        var worldRect = GetCameraViewWorldRect();
        windowCamera.orthographicSize = worldRect.height / 2.0f;// ( desktopRef.transform as RectTransform ).rect.width / ( image.transform as RectTransform ).rect.width;
        windowCamera.aspect = ( image.transform as RectTransform ).rect.width / ( image.transform as RectTransform ).rect.height;

        var windowViewPosWorld = worldRect.center.ToVector3( desktopRef.transform.position.z );
        var offset = ( windowViewPosWorld - desktopRef.windowCameraStartPosition ) - desktopRef.transform.position;
        windowCamera.transform.position = desktopRef.windowCameraStartPosition + offset;
    }
}