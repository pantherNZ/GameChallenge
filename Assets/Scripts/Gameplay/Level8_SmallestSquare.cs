using System.Collections.Generic;
using UnityEngine;

public class Level8_SmallestSquare : BaseLevel
{
    [SerializeField] GameObject squarePrefab = null;
    [SerializeField] int count = 8;
    List<GameObject> squares = new List<GameObject>();

    public override void OnStartLevel()
    {
        for( int i = 0; i < count; ++i )
        {
            var position = desktop.GetWorldBound( 1.0f ).RandomPosition(); 
            squares.Add( Instantiate( squarePrefab, position, Quaternion.identity ) );
            bool isSquare = Random.Range( 0, 2 ) == 0;
            var xScale = Random.Range( 0.5f, 3.0f );
            squares.Back().transform.localScale = new Vector3( xScale, isSquare ? xScale : Random.Range( 0.5f, 3.0f ), 1.0f );
        }
    }

    protected override void OnLevelFinished()
    {
        base.OnLevelFinished();

        squares.DestroyAll();
    }

    protected override void OnLevelUpdate()
    {
        base.OnLevelUpdate();
    }
}