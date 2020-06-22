using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1_BouncingBall : MonoBehaviour
{
    [SerializeField] GameObject ball = null;
    [SerializeField] GameObject platform = null;
    [SerializeField] bool test = false;

    public void StartLevel()
    {
        var desktop = GetComponent<DesktopUIManager>();
        desktop.CreateWindow();

        var newBall = Instantiate( ball, desktop.WindowCamera.transform );
        var newPlatform = Instantiate( platform, desktop.WindowCamera.transform ); 
        newBall.transform.localPosition = new Vector3( 8.0f, 4.0f, 100.0f );
        newPlatform.transform.localPosition = new Vector3( 4.0f, -2.0f, 100.0f );
        newBall.GetComponent<Rigidbody2D>().AddForce( new Vector2( -150.0f, 0.0f ) );
    }

    private void Start()
    {
        if( test )
            StartLevel();
    }
}
