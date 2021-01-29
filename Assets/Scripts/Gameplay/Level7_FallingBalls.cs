using System.Collections.Generic;
using UnityEngine;

public class Level7_FallingBalls : BaseLevel
{
    [SerializeField] GameObject ballPrefab = null;
    [SerializeField] int ballCountEasy = 20;
    [SerializeField] int ballCountHard = 30;
    [SerializeField] int numTargets = 3;
    [SerializeField] float xVelocityRange = 1.0f;
    [SerializeField] List<AudioClip> bounceAudio = new List<AudioClip>();
    [SerializeField] AudioClip selectAudio = null;

    List<GameObject> balls = new List<GameObject>();
    List<GameObject> targetBalls = new List<GameObject>();
    List<int> targets = new List<int>();
    Rect desktopBound;

    public override void OnStartLevel()
    {
        Utility.FunctionTimer.CreateTimer( 0.1f, CreateBall, "CreateBall", true );
        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_7_1" );

        for( int i = 0; i < numTargets; ++i )
        {
            int index;
            do
            {
                index = UnityEngine.Random.Range( 0, ( desktop.IsEasyMode() ? ballCountEasy : ballCountHard ) - 1 );
            }
            while( targets.Contains( index ) );

            targets.Add( index );
        }

        desktop.desktopSelectionEnabled = false;
    }

    void CreateBall()
    {
        if( balls.Count >= ( desktop.IsEasyMode() ? ballCountEasy : ballCountHard ) )
        {
            Utility.FunctionTimer.StopTimer( "CreateBall" );
            return;
        }

        desktopBound = desktop.GetWorldBound();
        var position = desktopBound.RandomPosition().ToVector3( 40.0f );
        balls.Add( Instantiate( ballPrefab, position, Quaternion.identity ) );
        balls.Back().GetComponent<Rigidbody2D>().AddForce( new Vector2( UnityEngine.Random.Range( -xVelocityRange, xVelocityRange ), 0.0f ) );
        balls.Back().GetComponent<EventDispatcher>().OnCollisionEnter2DEvent += ( collision ) =>
        {
            if( !balls.Contains( collision.gameObject ) )
                desktop.PlayAudio( bounceAudio.RandomItem(), 0.3f );
        };

        if( targets.Contains( balls.Count - 1 ) )
        {
            targets.Remove( balls.Count - 1 );
            balls.Back().GetComponent<SpriteRenderer>().color = Color.red;
            targetBalls.Add( balls.Back() );
        }
    }

    protected override void OnLevelUpdate()
    {
        base.OnLevelUpdate();

        foreach( var ball in balls )
        {
            if( ball == null )
                continue;

            if( ball.transform.position.y <= desktopBound.yMin )
            {
                ball.transform.position = ball.transform.position.SetY( desktopBound.yMax + 1.0f );
                var rigidBody = ball.GetComponent<Rigidbody2D>();
                rigidBody.velocity = rigidBody.velocity.SetY( 0.0f );
            }

            if( ball.transform.position.x < desktopBound.xMin )
                ball.transform.position = ball.transform.position.SetX( desktopBound.xMax );
            else if( ball.transform.position.x > desktopBound.xMax )
                ball.transform.position = ball.transform.position.SetX( desktopBound.xMin );
        }

        if( Input.GetMouseButton( 0 ) )
        {
            var mousePos = desktop.MainCamera.ScreenToWorldPoint( Input.mousePosition ).SetZ( 20.0f );
            var hit = Physics2D.Raycast( mousePos, Vector2.zero, 0.0f );

            if( hit.collider != null && targetBalls.Contains( hit.collider.gameObject ) )
            {
                targetBalls.Remove( hit.collider.gameObject );
                hit.collider.gameObject.Destroy();
                desktop.PlayAudio( selectAudio );
                CheckLevelComplete();
            }
        }
    }

    void CheckLevelComplete()
    {
        if( targetBalls.IsEmpty() )
        {
            SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_7_Complete" );
            Utility.FunctionTimer.CreateTimer( 1.0f, StartNextLevel );
        }
    }

    protected override void Cleanup( bool fromRestart )
    {
        base.Cleanup( fromRestart );

        Utility.FunctionTimer.StopTimer( "CreateBall" );

        desktop.desktopSelectionEnabled = true;

        balls.DestroyAll();
        targetBalls.DestroyAll();
        targets.Clear();
    }
}
