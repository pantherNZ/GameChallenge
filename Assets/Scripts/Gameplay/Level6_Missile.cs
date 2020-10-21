using System;
using System.Collections;
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
    [SerializeField] float missileStartPos = -6.0f;
    List<Window> windows = new List<Window>();
    GameObject shortcut, missileLauncher;
    List<Pair<GameObject, Coroutine>> missiles = new List<Pair<GameObject, Coroutine>>();

    public override void OnStartLevel()
    {
        var icon = new DesktopIcon()
        {
            name = "Missiles",
            icon = Resources.Load<Texture2D>( "Textures/Full_Recycle_Bin" )
        };

        shortcut = desktop.CreateShortcut( icon, new Vector2Int( 0, 1 ), CreateWindow );
        CreateWindow( shortcut );

         missileLauncher = Instantiate( missileLauncherPrefab );
        missileLauncher.transform.position = windows.Back().windowCamera.gameObject.transform.position + new Vector3( missileStartPos, 0.0f, 50.0f );
        missileLauncher.transform.localEulerAngles = new Vector3( 0.0f, 0.0f, -90.0f );

        Utility.FunctionTimer.CreateTimer( 3.0f, FireMissile, "FireMissile", true );

        SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_6_1" );
    }

    void CreateWindow( GameObject shortcut )
    {
        windows.Add( desktop.CreateWindow( "Missiles", false, new Vector2( -2.5f, 0.0f ) ).GetComponent<Window>() );
    }

    protected override void OnLevelUpdate()
    {
        base.OnLevelUpdate();

        foreach( var window in windows )
        {
            var rect = window.GetCameraViewWorldRect();

            foreach( var (missile, _) in missiles )
                if( !rect.Contains( missile.GetComponent<Collider2D>().bounds ) )
                    Explode( missile, false );
        }
    }

    protected override void OnLevelFinished()
    {
        base.OnLevelFinished();

        missileLauncher.Destroy();

        foreach( var (missile, _) in missiles )
            missile.Destroy();

        foreach( var window in windows)
            desktop.DestroyWindow( window );

        windows.Clear();
        missiles.Clear();

        Utility.FunctionTimer.CreateTimer( 1.0f, StartNextLevel );
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
            missiles.Add( new Pair<GameObject, Coroutine>( missile, StartCoroutine( MoveMissileRoutine( missile ) ) ) );
            Physics2D.SyncTransforms();
        } );
    }

    public IEnumerator MoveMissileRoutine( GameObject missile )
    {
        yield return Utility.InterpolatePosition( missile.transform, missile.transform.position + new Vector3( moveDist, 0.0f, 0.0f ), moveDist / moveSpeed );
        Explode( missile, true );
    }

    public void Explode( GameObject missile, bool reached_end )
    {
        if( missile == null )
            return;

        missile.GetComponent<Animator>().Play( "Explode" );
        StopCoroutine( missiles.FindPairFirst( missile ).Second );

        Utility.FunctionTimer.CreateTimer( 1.0f, () =>
        {
            if( missile != null )
            {
                missiles.RemovePairFirst( missile );
                missile.Destroy();
            }
        } );

        if( reached_end )
        {
            Utility.FunctionTimer.StopTimer( "FireMissile" );

            Utility.FunctionTimer.CreateTimer( 1.0f, () =>
            {
                LevelFinished();
                SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_6_Complete" );
            } );
        }
    }
}