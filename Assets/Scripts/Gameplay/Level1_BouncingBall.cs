using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1_BouncingBall : BaseLevel
{
    [SerializeField] GameObject ballPrefab = null;
    [SerializeField] GameObject platform = null;
    [SerializeField] GameObject goal = null;
    [SerializeField] Vector3 goalPosition = new Vector3( 7.5f, -1.5f, 50.0f );
    [SerializeField] Vector3 ballPosition = new Vector3( 2.0f, 1.5f, 50.0f );
    [SerializeField] float ballVelocity = 150.0f;
    [SerializeField] float ballFrequency = 2.0f;
    [SerializeField] float platformHeight = -1.5f;

    List<GameObject> objects = new List<GameObject>();
    bool alternate;
    bool[] complete = new bool[3];

    public override void OnStartLevel()
    {
        GetComponent<CanvasGroup>().SetVisibility( true );

        SubtitlesManager.Instance.QueueSubtitleGameString( 10.0f, "Narrator_Level_1_1" );

        var window = desktop.CreateWindow( "Bouncy Balls" ).GetComponent<Window>();

        var newPlatform = Instantiate( platform, window.windowCamera.gameObject.transform );
        newPlatform.transform.localPosition = new Vector3( 0.0f, platformHeight, 50.0f );
        objects.Add( newPlatform );

        var goal1 = Instantiate( goal, desktop.windowCameraStartPosition + goalPosition, Quaternion.identity );
        var goal2 = Instantiate( goal, desktop.windowCameraStartPosition + goalPosition.SetX( -goalPosition.x ), Quaternion.identity );
        goal1.GetComponent<EventDispatcher>().OnTriggerEnter2DEvent += ( Collider2D ) => { complete[0] = true; CheckComplete(); };
        goal2.GetComponent<EventDispatcher>().OnTriggerEnter2DEvent += ( Collider2D ) => { complete[1] = true; CheckComplete(); };
        goal1.GetComponent<EventDispatcher>().OnTriggerExit2DEvent += ( Collider2D ) => { complete[0] = false; };
        goal2.GetComponent<EventDispatcher>().OnTriggerExit2DEvent += ( Collider2D ) => { complete[1] = false; };
        objects.Add( goal1 );
        objects.Add( goal2 );

        CreateBall();

        objects.Add( Utility.CreateSprite( "Textures/Backgrounds/1_game_background", desktop.windowCameraStartPosition + new Vector3( 0.0f, 0.0f, 20.0f ), new Vector3( 1.5f, 1.5f ), Quaternion.identity, "SecondaryCamera" ) );

        Utility.FunctionTimer.CreateTimer( ballFrequency, CreateBall, "CreateBall", true );
    }

    private void CreateBall()
    {
        alternate = !alternate;

        // Don't create ball for side that is already complete
        if( ( !alternate && complete[0] ) || ( alternate && complete[1] ) )
            return;

        var newBall = Instantiate( ballPrefab, desktop.windowCameraStartPosition + ballPosition.SetX( alternate ? ballPosition.x : -ballPosition.x ), Quaternion.identity );
        newBall.GetComponent<Rigidbody2D>().AddForce( new Vector2( alternate ? -ballVelocity : ballVelocity, 0.0f ) );
        objects.Add( newBall );
    }

    protected override void OnLevelUpdate()
    {
        foreach( var ball in objects )
        {
            if( ball.transform.position.y <= -10.0f )
            {
                ball.Destroy();
                objects.Remove( ball );
                break;
            }
        }
    }

    private void CheckComplete()
    {
        if( complete[0] && complete[1] && !complete[2] )
        {
            complete[2] = true;
            SubtitlesManager.Instance.AddSubtitleGameString("Narrator_Level_1_Complete" );
            Utility.FunctionTimer.StopTimer( "CreateBall" );
            Utility.FunctionTimer.StopTimer( "Narrator_Level_1_2" );
            Utility.FunctionTimer.StopTimer( "Narrator_Level_1_1" );

            Utility.FunctionTimer.CreateTimer( 3.0f, () =>
            {
                objects.DestroyAll();
                desktop.DestroyWindow( "Bouncy Balls" );

                Utility.FunctionTimer.CreateTimer( 2.0f, StartNextLevel );
            } );
        }
    }

    public override string GetSpoilerText()
    {
        return DataManager.Instance.GetGameString( "Spoiler_Level1" );
    }
}
