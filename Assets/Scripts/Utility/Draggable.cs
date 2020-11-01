using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour
{
    bool dragging;
    Vector3 offset;
    new RectTransform transform;

    void Start()
    {
        transform = base.transform as RectTransform;
    }

    public void StartDrag( EventTrigger button )
    {
        if( dragging || !enabled )
            return;

        dragging = true;
        var targetPos = Input.mousePosition;
        targetPos.z = 150.0f;
        offset = transform.anchoredPosition - new Vector2( targetPos.x, targetPos.y );
    }

    public void EndDrag( EventTrigger button )
    {
        if( !dragging || !enabled )
            return;
        dragging = false;
    }

    public bool IsDragging()
    {
        return dragging;
    }

    private void Update()
    {
        if( dragging )
        {
            var targetPos = Input.mousePosition + offset;
            transform.anchoredPosition = targetPos;
            targetPos.z = 100.0f;
        }
    }
}