using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Level8_SmallestSquare : BaseLevel
{
    [SerializeField] GameObject squarePrefab = null;
    [SerializeField] int count = 8;
    [SerializeField] int stages = 3;
    [SerializeField] AudioClip selectAudio = null;
    List<GameObject> squares = new List<GameObject>();

    int stage = 1;
    GameObject smallest;

    public override void OnStartLevel()
    {
        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_8_1" );
        Setup();
    }

    private void Setup()
    {
        squares.DestroyAll();
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
            float yScale = xScale;

            while( !isSquare && Mathf.Abs( yScale - xScale ) <= 0.1f )
                yScale = Random.Range( 0.5f, 3.0f );

            squares.Back().transform.localScale = new Vector3( xScale, yScale, 1.0f );

            if( isSquare && xScale < smallestScale )
            {
                smallest = squares.Back();
                smallestScale = xScale;
            }
        }

        
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

                if( hit.collider.gameObject == smallest )
                {
                    desktop.PlayAudio( selectAudio );

                    if( stage < stages )
                    {
                        stage++;
                        Setup();
                    }
                    else
                    {
                        LevelFinished();
                    }
                }
                else
                {
                    desktop.GameOver();
                }
            }
        }
    }
}