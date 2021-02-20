using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level2_BridgeBall : BaseLevel
{
    [SerializeField] GameObject ballPrefab = null;
    [SerializeField] GameObject levelPrefab = null;
    [SerializeField] float ballOffset = -3.0f;
    [SerializeField] float ballVelocity = 140.0f;
    [SerializeField] float ballHeight = 5.0f;
    [SerializeField] float ballScale = 2.5f;
    [SerializeField] float ballFrequency = 2.0f;
    [SerializeField] AudioClip bounceAudio = null;
    [SerializeField] AudioClip inGoalAudio = null;
    List<GameObject> objects = new List<GameObject>();
    GameObject levelObj = null;
    Utility.FunctionTimer timer;

    public override void OnStartLevel()
    {
        GetComponent<CanvasGroup>().SetVisibility( true );
        Utility.FunctionTimer.CreateTimer( ballFrequency, CreateBall, "CreateBall", true );
        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_2_1" );

        levelObj = Instantiate( levelPrefab, new Vector3( 0.0f, 0.0f, 5.0f ), Quaternion.identity );

        var goal = levelObj.transform.GetChild( levelObj.transform.childCount - 1 );
        goal.GetComponent<EventDispatcher>().OnTriggerEnter2DEvent += ( Collider2D ) => { desktop.PlayAudio( inGoalAudio ); timer = Utility.FunctionTimer.CreateTimer( 1.0f, CheckComplete ); };
        goal.GetComponent<EventDispatcher>().OnTriggerExit2DEvent += ( Collider2D ) => { timer.Stop(); };

        if( desktop.IsEasyMode() )
        {
            levelObj.transform.GetChild( 2 ).gameObject.Destroy();
            //levelObj.transform.GetChild( 3 ).gameObject.Destroy();
        }
        else
        {
            levelObj.transform.GetChild( 1 ).gameObject.Destroy();
           // levelObj.transform.GetChild( 2 ).gameObject.Destroy();
            levelObj.transform.GetChild( 3 ).gameObject.Destroy();
        }
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

    private void CreateBall()
    {
        var newBall = Instantiate( ballPrefab, new Vector3( ballOffset, ballHeight, 5.0f ), Quaternion.identity );
        newBall.GetComponent<Rigidbody2D>().AddForce( new Vector2( ballVelocity, 0.0f ) );
        newBall.GetComponent<EventDispatcher>().OnCollisionEnter2DEvent += ( x ) => desktop.PlayAudio( bounceAudio );
        newBall.transform.localScale = new Vector2( ballScale, ballScale );
        newBall.transform.localRotation = Quaternion.Euler( 0.0f, 0.0f, UnityEngine.Random.Range( 0.0f, 360.0f ) );
        objects.Add( newBall );
    }

    private void CheckComplete()
    {
        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_2_Complete" );
        Utility.FunctionTimer.StopTimer( "CreateBall" );

        Utility.FunctionTimer.CreateTimer( 3.0f, () =>
        {
            LevelFinished( 2.0f );
        } );
    }

    protected override void Cleanup( bool fromRestart )
    {
        levelObj.Destroy();
        objects.DestroyAll();
        Utility.FunctionTimer.StopTimer( "CreateBall" );
    }
}