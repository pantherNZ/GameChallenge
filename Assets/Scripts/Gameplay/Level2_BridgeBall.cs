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
    [SerializeField] float successDelayTimer = 2.0f;
    List<GameObject> objects = new List<GameObject>();
    Utility.FunctionTimer timer;

    public override void StartLevel()
    {
        GetComponent<CanvasGroup>().SetVisibility( true );
        Utility.FunctionTimer.CreateTimer( ballFrequency, CreateBall, "CreateBall", true );

        var levelObj = Instantiate( levelPrefab, new Vector3( 0.0f, 0.0f, 5.0f ), Quaternion.identity );

        var goal = levelObj.transform.GetChild( levelObj.transform.childCount - 1 );
        goal.GetComponent<EventDispatcher>().OnTriggerEnter2DEvent += ( Collider2D ) => { timer = Utility.FunctionTimer.CreateTimer( successDelayTimer, CheckComplete ); };
        goal.GetComponent<EventDispatcher>().OnTriggerExit2DEvent += ( Collider2D ) => { timer.StopTimer(); };

        if( desktop.IsEasyMode() )
            levelObj.transform.GetChild( 2 ).gameObject.Destroy();
        else
        {
            levelObj.transform.GetChild( 1 ).gameObject.Destroy();
            levelObj.transform.GetChild( 3 ).gameObject.Destroy();
        }
    }

    private void Update()
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
        newBall.transform.localScale = new Vector2( ballScale, ballScale );
        objects.Add( newBall );
    }

    private void CheckComplete()
    {

    }
}