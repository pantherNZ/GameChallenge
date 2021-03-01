using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class Draggable : MonoBehaviour
{
    bool dragging;
    Vector3 offset;
    new RectTransform transform;
    public Action<Draggable, Vector3> updatePosition;

    void Start()
    {
        transform = base.transform as RectTransform;
    }

    public void StartDrag()
    {
        if( dragging || !enabled )
            return;

        dragging = true;
        var targetPos = DesktopUIManager.Instance.GetMousePosScreen();
        targetPos.z = 150.0f;
        offset = transform.anchoredPosition - new Vector2( targetPos.x, targetPos.y );
    }

    public void EndDrag()
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
            var targetPos = DesktopUIManager.Instance.GetMousePosScreen() + offset;
            if( updatePosition != null )
                updatePosition.Invoke( this, targetPos );
            else
                transform.anchoredPosition = targetPos;
            transform.SetAsLastSibling();
        }
    }
}