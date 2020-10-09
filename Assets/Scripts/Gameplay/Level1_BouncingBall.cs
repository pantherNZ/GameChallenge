using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1_BouncingBall : BaseLevel
{
    [SerializeField] GameObject ballPrefab = null;
    [SerializeField] float ballOffset = 3.0f;
    [SerializeField] float ballVelocity = 140.0f;
    [SerializeField] float ballHeight = 4.0f;
    [SerializeField] float ballFrequency = 2.0f;

    [SerializeField] GameObject platform = null;
    [SerializeField] GameObject goal = null;
    [SerializeField] float goalOffset = 10.0f;

    List<GameObject> objects = new List<GameObject>();
    bool alternate;
    bool[] complete = new bool[3];

    public override void OnStartLevel()
    {
        GetComponent<CanvasGroup>().SetVisibility( true );

        SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_2_1" ) );
        Utility.FunctionTimer.CreateTimer( 10.0f, () => { SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_2_2" ) ); }, "Narrator_Level_2_2" );

        desktop.CreateWindow( "Bouncy Balls" );

        var newPlatform = Instantiate( platform, desktop.WindowCamera.transform );
        newPlatform.transform.localPosition = new Vector3( 0.0f, -2.0f, 50.0f );
        objects.Add( newPlatform );

        var goal1 = Instantiate( goal, desktop.windowCameraStartPosition + new Vector3( -goalOffset, -4.0f, 50.0f ), Quaternion.identity );
        var goal2 = Instantiate( goal, desktop.windowCameraStartPosition + new Vector3( goalOffset, -4.0f, 50.0f ), Quaternion.identity );
        goal1.GetComponent<EventDispatcher>().OnTriggerEnter2DEvent += ( Collider2D ) => { complete[0] = true; CheckComplete(); };
        goal2.GetComponent<EventDispatcher>().OnTriggerEnter2DEvent += ( Collider2D ) => { complete[1] = true; CheckComplete(); };
        goal1.GetComponent<EventDispatcher>().OnTriggerExit2DEvent += ( Collider2D ) => { complete[0] = false; };
        goal2.GetComponent<EventDispatcher>().OnTriggerExit2DEvent += ( Collider2D ) => { complete[1] = false; };
        objects.Add( goal1 );
        objects.Add( goal2 );

        CreateBall();

        objects.Add( Utility.CreateSprite( "Textures/Backgrounds/1_game_background", desktop.windowCameraStartPosition + new Vector3( 0.0f, 0.0f, 20.0f ), new Vector3( 2.0f, 2.0f ), Quaternion.identity, "SecondaryCamera" ) );

        Utility.FunctionTimer.CreateTimer( ballFrequency, CreateBall, "CreateBall", true );
    }

    private void CreateBall()
    {
        alternate = !alternate;

        // Don't create ball for side that is already complete
        if( ( alternate && complete[0] ) || ( !alternate && complete[1] ) )
            return;

        var newBall = Instantiate( ballPrefab, desktop.windowCameraStartPosition + new Vector3( alternate ? ballOffset : -ballOffset, ballHeight, 50.0f ), Quaternion.identity );
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
            SubtitlesManager.Instance.AddSubtitle( DataManager.Instance.GetGameString( "Narrator_Level_2_Complete" ) );
            Utility.FunctionTimer.StopTimer( "CreateBall" );
            Utility.FunctionTimer.StopTimer( "Narrator_Level_2_2" );

            Utility.FunctionTimer.CreateTimer( 3.0f, () =>
            {
                foreach( var obj in objects )
                    obj.Destroy();
                objects.Clear();

                desktop.DestroyWindow( "Bouncy Balls" );

                Utility.FunctionTimer.CreateTimer( 2.0f, StartNextLevel );
            } );
        }
    }
}
