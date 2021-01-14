using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Level8_SmallestSquare : BaseLevel
{
    [SerializeField] GameObject squarePrefab = null;
    [SerializeField] int count = 8;
    [SerializeField] AudioClip selectAudio = null;
    List<GameObject> squares = new List<GameObject>();

    GameObject smallest;

    public override void OnStartLevel()
    {
        float smallestScale = float.MaxValue;

        for( int i = 0; i < count; ++i )
        {
            Vector2 position;
            int safety = 0;

            do
            {
                position = desktop.GetWorldBound( 1.0f ).RandomPosition();
            }
            while( ++safety < 20 && squares.Any( x => ( x.transform.position.ToVector2() - position ).sqrMagnitude <= 5.0f ) );

            squares.Add( Instantiate( squarePrefab, position.ToVector3( 10.0f ), Quaternion.identity ) );
            bool isSquare = i == 0 || Random.Range( 0, 2 ) == 0;
            var xScale = Random.Range( 0.5f, 3.0f );
            squares.Back().transform.localScale = new Vector3( xScale, isSquare ? xScale : Random.Range( 0.5f, 3.0f ), 1.0f );

            if( isSquare && xScale < smallestScale )
            {
                smallest = squares.Back();
                smallestScale = xScale;
            }
        }

        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_8_1" );
        //smallest.GetComponent<SpriteRenderer>().color = Color.red;
    }

    protected override void OnLevelFinished()
    {
        base.OnLevelFinished();

        squares.DestroyAll();
        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_8_Complete" );
        Utility.FunctionTimer.CreateTimer( 2.0f, StartNextLevel );
    }

    protected override void OnLevelUpdate()
    {
        base.OnLevelUpdate();

        if( Input.GetMouseButton( 0 ) )
        {
            var mousePos = desktop.MainCamera.ScreenToWorldPoint( Input.mousePosition ).SetZ( 20.0f );
            var hit = Physics2D.Raycast( mousePos, Vector2.zero, 0.0f );

            if( hit.collider != null && squares.Contains( hit.collider.gameObject ) )
            {
                squares.Remove( hit.collider.gameObject );
                hit.collider.gameObject.Destroy();
                desktop.PlayAudio( selectAudio );

                if( hit.collider.gameObject == smallest )
                {
                    LevelFinished();
                }
                else
                {
                    desktop.GameOver();
                }
            }
        }
    }
}