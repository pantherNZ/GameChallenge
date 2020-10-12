using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Level6_Missile : BaseLevel
{
    [SerializeField] GameObject missileLauncherPrefab = null;
    [SerializeField] GameObject missilePrefab = null;
    [SerializeField] float moveDist = 20.0f;
    [SerializeField] float moveSpeed = 10.0f;
    List<GameObject> windows = new List<GameObject>();
    GameObject shortcut, missileLauncher;
    List<GameObject> missiles = new List<GameObject>();

    public override void OnStartLevel()
    {
        windows.Add( desktop.CreateWindow( "Missiles" ) );
        var icon = new DesktopIcon()
        {
            name = "Missiles",
            icon = Resources.Load<Texture2D>( "Textures/Full_Recycle_Bin" )
        };
        shortcut = desktop.CreateShortcut( icon, new Vector2Int( 0, 1 ), ( x ) => windows.Add( desktop.CreateWindow( "Missiles" ) ) );

        missileLauncher = Instantiate( missileLauncherPrefab, windows.Back().GetComponent<Window>().windowCamera.gameObject.transform );
        missileLauncher.transform.localPosition = new Vector3( -3.5f, 0.0f, 50.0f );
        missileLauncher.transform.localEulerAngles = new Vector3( 0.0f, 0.0f, -90.0f );

        Utility.FunctionTimer.CreateTimer( 3.0f, FireMissile, "FireMissile", true );
    }

    protected override void OnLevelUpdate()
    {
        base.OnLevelUpdate();
    }

    void FireMissile()
    {
        missileLauncher.GetComponent<Animator>().Play( "Fire" );

        Utility.FunctionTimer.CreateTimer( 0.5f, () =>
        {
            var missile = Instantiate( missilePrefab );
            var spawnLocation = missileLauncher.GetComponentInChildren<Transform>();
            missile.transform.position = spawnLocation.position;
            missile.transform.rotation = spawnLocation.rotation;
            StartCoroutine( MoveMissileRoutine( missile ) );
            missiles.Add( missile );
        } );
    }

    public System.Collections.IEnumerator MoveMissileRoutine( GameObject missile )
    {
        yield return StartCoroutine( Utility.InterpolatePosition( missile.transform, missile.transform.position + new Vector3( moveDist, 0.0f, 0.0f ), moveDist / moveSpeed ) );
        missile.GetComponent<Animator>().Play( "Explode" );

        Utility.FunctionTimer.CreateTimer( 1.0f, () =>
        {
            if( missile != null )
            {
                missiles.Remove( missile );
                missile.Destroy();
            }
        } );
    }
}