using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Window : MonoBehaviour
{
    bool dragging;
    Vector3 offset;
    new Camera camera;
    new RectTransform transform;
    [SerializeField] GameObject cameraView = null;
    [SerializeField] Text titleText = null;
    [SerializeField] GameObject child = null;

    void Start()
    {
        camera = Camera.main;
        transform = base.transform as RectTransform;
    }

    public void SetTitle( string title )
    {
        titleText.text = title;
    }

    public void StartDrag( EventTrigger button )
    {
        if( dragging )
            return;

        dragging = true;
        var targetPos = Input.mousePosition;
        targetPos.z = 150.0f;
        offset = transform.anchoredPosition - new Vector2( targetPos.x, targetPos.y );
    }
     
    public void EndDrag( EventTrigger button )
    {
        if( !dragging )
            return;
        dragging = false;
    }

    private void Update()
    {
        if( dragging )
        {
            var targetPos = Input.mousePosition + offset;
            transform.anchoredPosition = targetPos;
            targetPos.z = 100.0f;

            if( child != null )
                child.transform.position = targetPos;
        }
    }

    public void GetCameraViewWorldCorners( Vector3[] corners )
    {
        ( cameraView.transform as RectTransform ).GetWorldCorners( corners );
    }
}