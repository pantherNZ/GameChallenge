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
    public Vector2 rootPos = new Vector2( 0.0f, 0.0f );
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
                var oldPos = rectTransform.localPosition.ToVector2();
                var pos = oldPos - rootPos;
                var coord = new Vector2( Mathf.Floor( pos.x / gridWidth ), Mathf.Floor( pos.y / gridHeight ) );
                var x = Mathf.Clamp( rootPos.x + coord.x * gridWidth + gridWidth / 2.0f, minPos.x, maxPos.x );
                var y = Mathf.Clamp( rootPos.y + coord.y * gridHeight + gridHeight / 2.0f, minPos.y, maxPos.y );
                pos = new Vector3( x, y, rectTransform.position.z );
                rectTransform.localPosition = pos;

                if( disableOverlapWithTag.Length > 0 && ( overlap || ( oldPos - rectTransform.localPosition.ToVector2() ).sqrMagnitude > 0.001f ) )
                {
                    overlap = false;

                    for( int i = 0; i < transform.parent.childCount; ++i )
                    {
                        var child = transform.parent.GetChild( i );

                        if( child.gameObject != gameObject && child.CompareTag( disableOverlapWithTag ) && ( child.transform as RectTransform ).Overlaps( rectTransform ) )
                        {
                            var atBottom = rectTransform.localPosition.y - gridHeight <= minPos.y;
                            var newPos = rectTransform.localPosition.ToVector2() + new Vector2( atBottom ? gridWidth : 0.0f, atBottom ? -rectTransform.localPosition.y : -gridHeight );
                            rectTransform.localPosition = newPos;
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
