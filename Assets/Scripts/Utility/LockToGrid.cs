using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockToGrid : MonoBehaviour
{
    [SerializeField] float gridWidth = 1.0f;
    [SerializeField] float gridHeight = 1.0f;

    void Update()
    {
        var rectTransform = transform as RectTransform;
        if( rectTransform != null )
        {
            var pos = rectTransform.localPosition;
            rectTransform.localPosition = new Vector3( pos.x - ( pos.x % gridWidth ), pos.y - ( pos.y % gridHeight ), pos.z );
        }
    }
}
