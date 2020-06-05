using UnityEngine;
using UnityEngine.EventSystems;

public class TestWindow : MonoBehaviour
{
    bool dragging;
    Vector3 offset;
    new Camera camera;
    new RectTransform transform;
    public GameObject child = null;

    void Start()
    {
        camera = Camera.main;
        transform = base.transform as RectTransform;
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
}