using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockToGrid : MonoBehaviour
{
    public float cellWidth = 1.0f;
    public float cellHeight = 1.0f;
    public int gridWidth;
    public int gridHeight;
    public string disableOverlapWithTag;
    Vector2 minPos = new Vector2( float.NegativeInfinity, float.NegativeInfinity );
    Vector2 maxPos = new Vector2( float.PositiveInfinity, float.PositiveInfinity );
    public Vector2 rootPos = new Vector2( 0.0f, 0.0f );
    public Action<GameObject> onOverlapWith;

    public Vector2 MinPos
    {
        get => minPos;
        set
        {
            minPos = value;
            UpdateGridSize();
        }
    }
    public Vector2 MaxPos
    {
        get => maxPos;
        set
        {
            maxPos = value;
            UpdateGridSize();
        }
    }

    private void UpdateGridSize()
    {
        var invalid = new Vector2( float.NegativeInfinity, float.NegativeInfinity );
        if( minPos != invalid && maxPos != invalid )
        {
            gridWidth = Mathf.CeilToInt( ( Mathf.Max( maxPos.x, minPos.x ) - Mathf.Min( maxPos.x, minPos.x ) ) / cellWidth );
            gridHeight = Mathf.CeilToInt( ( Mathf.Max( maxPos.y, minPos.y ) - Mathf.Min( maxPos.y, minPos.y ) ) / cellHeight );
        }
    }

    public void SetGridPosition( Vector2Int coord, bool checkOverlaps = true )
    {
        var oldPos = transform.localPosition.ToVector2();
        var x = Mathf.Clamp( rootPos.x + coord.x * cellWidth + cellWidth / 2.0f, minPos.x, maxPos.x );
        var y = Mathf.Clamp( rootPos.y + coord.y * cellHeight + cellHeight / 2.0f, minPos.y, maxPos.y );
        transform.localPosition = new Vector3( x, y, transform.position.z );

        if( ( oldPos - transform.localPosition.ToVector2() ).sqrMagnitude > 0.001f && checkOverlaps )
            CheckOverlaps();
    }

    void Update()
    {
        var pos = transform.localPosition.ToVector2() - rootPos;
        var coord = new Vector2Int( Mathf.FloorToInt( pos.x / cellWidth ), Mathf.FloorToInt( pos.y / cellHeight ) );
        SetGridPosition( coord );
    }

    void CheckOverlaps()
    {
        var rectTransform = transform as RectTransform;
        int attempts = 0;
        bool overlap = true;

        do
        {
            var oldPos = rectTransform.localPosition.ToVector2();
            var pos = oldPos - rootPos;
            var coord = new Vector2Int( Mathf.FloorToInt( pos.x / cellWidth ), Mathf.FloorToInt( pos.y / cellHeight ) );
            SetGridPosition( coord, false );

            if( disableOverlapWithTag.Length > 0 && ( overlap || ( oldPos - rectTransform.localPosition.ToVector2() ).sqrMagnitude > 0.001f ) )
            {
                overlap = false;

                for( int i = 0; i < transform.parent.childCount; ++i )
                {
                    var child = transform.parent.GetChild( i );

                    if( child.gameObject != gameObject && child.CompareTag( disableOverlapWithTag ) && ( child.transform as RectTransform ).Overlaps( rectTransform ) )
                    {
                        var atBottom = rectTransform.localPosition.y <= minPos.y;
                        var newPos = rectTransform.localPosition.ToVector2() + new Vector2( atBottom ? cellWidth : 0.0f, atBottom ? rootPos.y + Mathf.Max( maxPos.y, minPos.y ) : -cellHeight );
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
