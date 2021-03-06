using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

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
            var square = Instantiate( squarePrefab, new Vector3(), Quaternion.identity, desktop.DesktopCanvas );

            var squareTransform = square.transform as RectTransform;
            bool isSquare = i == 0 || Random.Range( 0, 2 ) == 0;
            var xScale = Random.Range( 0.5f, 3.0f );
            float yScale = xScale;

            while( !isSquare && Mathf.Abs( yScale - xScale ) <= 0.1f )
                yScale = Random.Range( 0.5f, 3.0f );

            squareTransform.localScale = new Vector3( xScale, yScale, 1.0f );

            if( isSquare && xScale < smallestScale )
            {
                smallest = square;
                smallestScale = xScale;
            }

            int safety = 0;
            do
            {
                var pos = desktop.GetScreenBound( 200.0f ).RandomPosition().ToVector3( 10.0f );
                //pos -= new Vector3( ( desktop.transform as RectTransform ).rect.width / 2.0f, ( desktop.transform as RectTransform ).rect.height / 2.0f, 0.0f );
                //squareTransform.anchoredPosition = pos.SetZ( 0.0f );
                squareTransform.localPosition = pos.SetZ( 0.0f );
                squareTransform.ForceUpdateRectTransforms();
            }
            while( ++safety < 20 && squares.Any( x => ( ( x.transform as RectTransform ).GetWorldRect().Overlaps( squareTransform.GetWorldRect() ) ) ) );

            square.GetComponent<Button>().onClick.AddListener( () => OnClick( square ) );
            squares.Add( square );
        }

        
        //smallest.GetComponent<SpriteRenderer>().color = Color.red;
    }

    protected override void Cleanup( bool fromRestart )
    {
        base.Cleanup( fromRestart );

        squares.DestroyAll();
        stage = 1;
    }

    protected override void OnLevelFinished()
    {
        base.OnLevelFinished();

        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_8_Complete" );
        Utility.FunctionTimer.CreateTimer( 2.0f, StartNextLevel );
    }

    private void OnClick( GameObject square )
    {
        squares.Remove( square );
        square.Destroy();

        if( square == smallest )
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
            desktop.LevelFailed();
        }
    }
}