using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1_BouncingBall : MonoBehaviour
{
    [SerializeField] GameObject ball = null;
    [SerializeField] float ballOffset = 3.0f;
    [SerializeField] float ballVelocity = 140.0f;
    [SerializeField] float ballHeight = 4.0f;

    [SerializeField] GameObject platform = null;
    [SerializeField] GameObject goal = null;
    [SerializeField] float goalOffset = 10.0f;
    [SerializeField] bool test = false;

    List<GameObject> objects = new List<GameObject>();
    DesktopUIManager desktop;
    bool alternate;
    int overlaps = 0;

    public void StartLevel()
    {
        desktop = GetComponent<DesktopUIManager>();
        desktop.CreateWindow( "Bouncy Balls" );

        var newPlatform = Instantiate( platform, desktop.WindowCamera.transform );
        newPlatform.transform.localPosition = new Vector3( 0.0f, -2.0f, 50.0f );
        objects.Add( newPlatform );

        var goal1 = Instantiate( goal, desktop.windowCameraStartPosition + new Vector3( -goalOffset, -4.0f, 50.0f ), Quaternion.identity );
        var goal2 = Instantiate( goal, desktop.windowCameraStartPosition + new Vector3( goalOffset, -4.0f, 50.0f ), Quaternion.identity );
        goal1.GetComponent<EventDispatcher>().OnTriggerEnter2DEvent += ( Collider2D ) => { overlaps++; CheckComplete(); };
        goal2.GetComponent<EventDispatcher>().OnTriggerEnter2DEvent += ( Collider2D ) => { overlaps++; CheckComplete(); };
        goal1.GetComponent<EventDispatcher>().OnTriggerExit2DEvent += ( Collider2D ) => { overlaps--; };
        goal2.GetComponent<EventDispatcher>().OnTriggerExit2DEvent += ( Collider2D ) => { overlaps--; };
        objects.Add( goal1 );
        objects.Add( goal2 );

        CreateBall();

        var background = new GameObject();
        background.transform.position = desktop.windowCameraStartPosition + new Vector3( 0.0f, 0.0f, 20.0f );
        var spriteRenderer = background.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Resources.Load<Sprite>( "Textures/Backgrounds/1_game_background" );
        spriteRenderer.sortingOrder = -1;
        background.transform.localScale = new Vector3( 2.0f, 2.0f, 1.0f );
        background.layer = LayerMask.NameToLayer( "SecondaryCamera" );
        objects.Add( background );

        Utility.FunctionTimer.CreateTimer( 2.0f, CreateBall, "CreateBall", true );
    }

    private void CreateBall()
    {
        var newBall = Instantiate( ball, desktop.windowCameraStartPosition + new Vector3( alternate ? ballOffset : -ballOffset, ballHeight, 50.0f ), Quaternion.identity );
        newBall.GetComponent<Rigidbody2D>().AddForce( new Vector2( alternate ? -ballVelocity : ballVelocity, 0.0f ) );
        objects.Add( newBall );

        alternate = !alternate;
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

    private void Start()
    {
        if( test )
            StartLevel();
    }

    private void CheckComplete()
    {
        if( overlaps >= 2 )
        {
            foreach( var obj in objects )
                obj.Destroy();
            objects.Clear();
            Utility.FunctionTimer.StopTimer( "CreateBall" );
            desktop.DestroyWindow( "Bouncy Balls" );
        }
    }
}
