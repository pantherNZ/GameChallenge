using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockToGrid : MonoBehaviour
{
    public float gridWidth = 1.0f;
    public float gridHeight = 1.0f;
    public string disableOverlapWithTag;
    public Vector2 minPos = new Vector2( float.NegativeInfinity, float.NegativeInfinity );
    public Vector2 maxPos = new Vector2( float.PositiveInfinity, float.PositiveInfinity );
    public Action<GameObject> onOverlapWith;

    void Update()
    {
        var rectTransform = transform as RectTransform;
        if( rectTransform != null )
        {
            int attempts = 0;
            bool overlap = false;

            do
            {
                var pos = rectTransform.anchoredPosition;
                rectTransform.anchoredPosition = new Vector2(
                    Mathf.Clamp( ( Mathf.Floor( ( pos.x + gridWidth / 2.0f ) / gridWidth ) ) * gridWidth, minPos.x, maxPos.x ),
                    Mathf.Clamp( ( Mathf.Floor( ( pos.y + gridHeight / 2.0f ) / gridHeight ) ) * gridHeight, minPos.y, maxPos.y ) );

                if( disableOverlapWithTag.Length > 0 && ( overlap || ( pos - rectTransform.anchoredPosition ).sqrMagnitude > 0.001f ) )
                {
                    overlap = false;

                    for( int i = 0; i < transform.parent.childCount; ++i )
                    {
                        var child = transform.parent.GetChild( i );

                        if( child.gameObject != gameObject && child.CompareTag( disableOverlapWithTag ) && ( child.transform as RectTransform ).Overlaps( rectTransform ) )
                        {
                            var atBottom = rectTransform.anchoredPosition.y - gridHeight <= minPos.y;
                            var newPos = rectTransform.anchoredPosition + new Vector2( atBottom ? gridWidth : 0.0f, atBottom ? -rectTransform.anchoredPosition.y : -gridHeight );
                            rectTransform.anchoredPosition = newPos;
                            onOverlapWith?.Invoke( child.gameObject );
                            overlap = true;
                            break;
                        }
                    }
                }
            }
            while( overlap && ++attempts <= 1000 );
        }
    }
}
