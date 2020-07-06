using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockToGrid : MonoBehaviour
{
    public float gridWidth = 1.0f;
    public float gridHeight = 1.0f;
    public Vector2 minPos = new Vector2( float.NegativeInfinity, float.NegativeInfinity );
    public Vector2 maxPos = new Vector2( float.PositiveInfinity, float.PositiveInfinity );

    void Update()
    {
        var rectTransform = transform as RectTransform;
        if( rectTransform != null )
        {
            var pos = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2( 
                Mathf.Clamp( ( Mathf.Floor( ( pos.x + gridWidth / 2.0f ) / gridWidth ) ) * gridWidth, minPos.x, maxPos.x ),
                Mathf.Clamp( ( Mathf.Floor( ( pos.y + gridHeight / 2.0f ) / gridHeight ) ) * gridHeight, minPos.y, maxPos.y ) );
        }
    }
}
