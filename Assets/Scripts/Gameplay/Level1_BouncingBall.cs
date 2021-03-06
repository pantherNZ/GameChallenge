﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] AudioClip bounceAudio = null;
    [SerializeField] AudioClip inGoalAudio = null;

    List<GameObject> objects = new List<GameObject>();
    [SerializeField] GameObject flag = null;
    GameObject shortcut, window;
    bool alternate;
    bool[] complete = new bool[3];

    private void Start()
    {
        flag.GetComponent<CanvasGroup>().SetVisibility( false );
    }

    public override void OnStartLevel()
    {
        complete = new bool[3];
        GetComponent<CanvasGroup>().SetVisibility( true );

        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_1_1" );
        SubtitlesManager.Instance.QueueSubtitleGameString( 5.0f, "Narrator_Level_1_2" );

        objects.DestroyAll();
        var newPlatform = Instantiate( platform );
        objects.Add( newPlatform );
        CreateWindow();

        newPlatform.transform.localPosition = new Vector3( 0.0f, platformHeight, 50.0f );

        var goal1 = Instantiate( goal, desktop.windowCameraStartPosition + goalPosition, Quaternion.identity );
        var goal2 = Instantiate( goal, desktop.windowCameraStartPosition + goalPosition.SetX( -goalPosition.x ), Quaternion.identity );
        goal1.GetComponent<EventDispatcher>().OnTriggerEnter2DEvent += ( Collider2D ) => { complete[0] = true; CheckComplete(); };
        goal2.GetComponent<EventDispatcher>().OnTriggerEnter2DEvent += ( Collider2D ) => { complete[1] = true; CheckComplete(); };
        goal1.GetComponent<EventDispatcher>().OnTriggerExit2DEvent += ( Collider2D ) => { complete[0] = false; };
        goal2.GetComponent<EventDispatcher>().OnTriggerExit2DEvent += ( Collider2D ) => { complete[1] = false; };
        objects.Add( goal1 );
        objects.Add( goal2 );

        objects.Add( Utility.CreateSprite( "Textures/Backgrounds/1_game_background", desktop.windowCameraStartPosition + new Vector3( 0.0f, 0.0f, 20.0f ), new Vector3( 1.5f, 1.5f ), Quaternion.identity, "SecondaryCamera" ) );

        Utility.FunctionTimer.CreateTimer( ballFrequency, CreateBall, "CreateBall", true );

        shortcut = desktop.CreateShortcut( new DesktopIcon() { icon = startMenuEntryIcon, name = startMenuEntryText }, new Vector2Int( 0, 1 ), ( obj ) => CreateWindow() );
    }

    private void CreateWindow()
    {
        if( window != null )
            return;

        var windowCmp = desktop.CreateWindow( "Bouncy Balls", true ).GetComponent<Window>();

        windowCmp.onClose += ( _ ) => 
        {
            if( flag != null )
            {
                flag.GetComponent<CanvasGroup>().SetVisibility( false );
                flag.transform.SetParent( desktop.DesktopCanvas, true );
            }

            objects[0].transform.SetParent( desktop.DesktopCanvas, true );
        };

        window = windowCmp.gameObject;
        window.AddComponent<RectMask2D>();
        window.GetComponent<Draggable>().updatePosition = ( _, pos ) =>
        {
            if( flag != null ) flag.transform.SetParent( desktop.DesktopCanvas, true );
            ( window.transform as RectTransform ).anchoredPosition = pos;
            if( flag != null ) flag.transform.SetParent( window.transform, true );
        };

        if( flag != null )
        {
            flag.transform.SetParent( window.transform, true );
            flag.GetComponent<CanvasGroup>().SetVisibility( true );
        }

        objects[0].transform.SetParent( windowCmp.windowCamera.gameObject.transform, true );
    }

    private void CreateBall()
    {
        alternate = !alternate;

        // Don't create ball for side that is already complete
        if( ( !alternate && complete[0] ) || ( alternate && complete[1] ) )
            alternate = !alternate;

        var newBall = Instantiate( ballPrefab, desktop.windowCameraStartPosition + ballPosition.SetX( alternate ? ballPosition.x : -ballPosition.x ), Quaternion.identity );
        newBall.GetComponent<Rigidbody2D>().AddForce( new Vector2( alternate ? -ballVelocity : ballVelocity, 0.0f ) );
        newBall.GetComponent<EventDispatcher>().OnCollisionEnter2DEvent += ( x ) => desktop.PlayAudio( bounceAudio );
        newBall.transform.localRotation = Quaternion.Euler( 0.0f, 0.0f, Random.Range( 0.0f, 360.0f ) );
        objects.Add( newBall );
    }

    protected override void OnLevelUpdate()
    {
        base.OnLevelUpdate();

        objects.RemoveAll( x => x == null );

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

    protected override void OnLevelFinished()
    {
        base.OnLevelFinished();

        Utility.FunctionTimer.CreateTimer( 5.0f, () =>
        {
            StartCoroutine( desktop.RunTimer() );
            Utility.FunctionTimer.CreateTimer( 1.0f, () =>
            {
                SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Start_Update" );
            } );
        } );
    }

    protected override void Cleanup( bool fromRestart )
    {
        base.Cleanup( fromRestart );

        Utility.FunctionTimer.StopTimer( "CreateBall" );
        Utility.FunctionTimer.StopTimer( "Narrator_Level_1_2" );
        Utility.FunctionTimer.StopTimer( "Narrator_Level_1_1" );

        if( fromRestart )
        {
            objects.DestroyAll();
            objects.Clear();
        }

        if( flag != null )
            flag.GetComponent<CanvasGroup>().SetVisibility( false );

        desktop.RemoveShortcut( shortcut );
        desktop.DestroyWindow( window );
    }

    private void CheckComplete()
    {
        if( complete[0] || complete[1] )
            desktop.PlayAudio( inGoalAudio );

        if( complete[0] && complete[1] && !complete[2] )
        {
            complete[2] = true;
            SubtitlesManager.Instance.AddSubtitleGameString("Narrator_Level_1_Complete" );
            LevelFinished( 11.0f );

            Utility.FunctionTimer.CreateTimer( 2.0f, () =>
            {
                objects.DestroyAll();
                desktop.DestroyWindowByTitle( "Bouncy Balls" );
            } );
        }
    }
}
