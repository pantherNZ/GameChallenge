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
    [SerializeField] GameObject missileBezierPrefab = null;
    [SerializeField] GameObject missileBezier2Prefab = null;
    [SerializeField] GameObject missileBezier3Prefab = null;
    [SerializeField] float moveDist = 20.0f;
    [SerializeField] float moveSpeed = 30.0f;
    [SerializeField] float moveSpeed2 = 10.0f;
    [SerializeField] float missileStartPos = -6.0f;
    [SerializeField] Vector2 windowStartPos = new Vector2( -3.0f, 0.0f );
    [SerializeField] AudioClip fireAudio = null;
    [SerializeField] AudioClip explodeAudio = null;
    List<Window> windows = new List<Window>();
    GameObject shortcut, missileLauncher;
    List<Pair<GameObject, Coroutine>> missiles = new List<Pair<GameObject, Coroutine>>();
    int levelCounter = 0;
    bool fireLeft, firstMissileSuccess;
    int maxWindows;

    public override void OnStartLevel()
    {
        maxWindows = desktop.IsEasyMode() ? 4 : 2;

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
        levelCounter = 2;
        SetupLevel();
    }

    void CreateWindow( GameObject shortcut )
    {
        windows.RemoveAll( x => x == null );

        if( windows.Count >= maxWindows )
            return;

        windows.Add( desktop.CreateWindow( "Missiles", false, windowStartPos ).GetComponent<Window>() );
        windowStartPos += new Vector2( 0.3f, windowStartPos.y > -2.0f ? -0.3f : 0.0f );
    }

    protected override void OnLevelUpdate()
    {
        base.OnLevelUpdate();

        foreach( var (missile, _) in missiles )
        {
            var found = windows.Any( window =>
            {
                var rect = window.GetCameraViewWorldRect();
                return rect.Contains( missile.GetComponent<Collider2D>().bounds );
            } );

            if( !found )
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

        Utility.FunctionTimer.StopTimer( "FireMissile" );
        Utility.FunctionTimer.StopTimer( "FireMissile2" );
        Utility.FunctionTimer.CreateTimer( 1.0f, StartNextLevel );
    }

    void FireMissile()
    {
        missileLauncher.GetComponent<Animator>().Play( "Fire" );
        desktop.PlayAudio( fireAudio );
        firstMissileSuccess = false;

        Utility.FunctionTimer.CreateTimer( 0.5f, () =>
        {
            var missile = Instantiate( missilePrefab );
            var spawnLocation = missileLauncher.GetComponentInChildren<Transform>();
            missile.transform.position = spawnLocation.position;
            missile.transform.rotation = spawnLocation.rotation;
            missiles.Add( new Pair<GameObject, Coroutine>( missile, StartCoroutine( MoveMissileRoutine( missile ) ) ) );
            Physics2D.SyncTransforms();
        } );

        if( levelCounter == 2 && !fireLeft )
            Utility.FunctionTimer.CreateTimer( 0.75f, FireMissile, "FireMissile2" );
        fireLeft = !fireLeft;
    }

    public IEnumerator MoveMissileRoutine( GameObject missile )
    {
        switch( levelCounter )
        {
            case 0:
                yield return Utility.InterpolatePosition( missile.transform, missile.transform.position + new Vector3( moveDist, 0.0f, 0.0f ), moveDist / moveSpeed );
                break;
            case 1:
                yield return Utility.InterpolateAlongPath( missile.transform, missileBezierPrefab.GetComponent<PathCreation.PathCreator>(), moveDist / moveSpeed2 );
                break;
            case 2:
            {
                if( fireLeft )
                    yield return Utility.InterpolateAlongPath( missile.transform, missileBezier2Prefab.GetComponent<PathCreation.PathCreator>(), moveDist / moveSpeed2 );
                else
                    yield return Utility.InterpolateAlongPath( missile.transform, missileBezier3Prefab.GetComponent<PathCreation.PathCreator>(), moveDist / moveSpeed2 );
                break;
            }
        }

        Explode( missile, true );
    }

    public void Explode( GameObject missile, bool reached_end )
    {
        if( missile == null )
            return;

        missile.GetComponent<Animator>().Play( "Explode" );
        desktop.PlayAudio( explodeAudio );
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
            if( levelCounter == 2 )
            {
                if( !firstMissileSuccess )
                {
                    firstMissileSuccess = true;
                    return;
                }
            }

            Utility.FunctionTimer.CreateTimer( 1.0f, () =>
            {
                ++levelCounter;
                SetupLevel();
            } );
        }
        else
        {
            firstMissileSuccess = false;
        }
    }

    void SetupLevel()
    {
        switch( levelCounter )
        {
            case 0:
                SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_6_1" );
                break;
            case 1:
                SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_6_2" );
                break;
            case 2:
                SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_6_3" );
                break;
            case 3:
            {
                LevelFinished();
                SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_6_Complete" );
                break;
            }
        }
    }
}