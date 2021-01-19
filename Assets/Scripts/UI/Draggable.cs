using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour
{
    bool dragging;
    Vector3 offset;
    new RectTransform transform;
    Transform parentRef;

    void Start()
    {
        transform = base.transform as RectTransform;
    }

    public void StartDrag()
    {
        StartDrag( null );
    }

    public void StartDrag( Transform parent )
    {
        if( dragging || !enabled )
            return;

        dragging = true;
        parentRef = parent;
        var targetPos = GetMousePosScreen();
        targetPos.z = 150.0f;
        offset = transform.anchoredPosition - new Vector2( targetPos.x, targetPos.y );
    }

    public Vector3 GetMousePosScreen()
    {
        var centre = new Vector3( Screen.width, Screen.height, 0.0f ) / 2.0f;
        return ( Input.mousePosition - centre ).RotateZ( parentRef != null ? parentRef.rotation.eulerAngles.z : 0.0f ) + centre;
    }

    public void EndDrag()
    {
        if( !dragging || !enabled )
            return;
        dragging = false;
        parentRef = null;
    }

    public bool IsDragging()
    {
        return dragging;
    }

    private void Update()
    {
        if( dragging )
        {
            var targetPos = GetMousePosScreen() + offset;
            transform.anchoredPosition = targetPos;
            targetPos.z = 100.0f;
            transform.SetAsLastSibling();
        }
    }
}