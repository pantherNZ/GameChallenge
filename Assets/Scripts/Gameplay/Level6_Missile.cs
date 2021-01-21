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
    Vector2 nextWindowStartPos;
    [SerializeField] AudioClip fireAudio = null;
    [SerializeField] AudioClip explodeAudio = null;
    [SerializeField] AudioClip stageCompleteAudio = null;
    List<Window> windows = new List<Window>();
    GameObject shortcut, missileLauncher, background;

    class Missile
    {
        public GameObject missile;
        public Coroutine movement;
        public int index;
    }

    List<Missile> missiles = new List<Missile>();
    int levelCounter;
    bool fireLeft;
    int maxWindows;
    int index;
    int lastHitIndex = -1;

    public override void OnStartLevel()
    {
        maxWindows = desktop.IsEasyMode() ? 4 : 2;

        var icon = new DesktopIcon()
        {
            name = "Missiles",
            icon = Resources.Load<Texture2D>( "Textures/Full_Recycle_Bin" )
        };

        levelCounter = 0;
        shortcut = desktop.CreateShortcut( icon, new Vector2Int( 0, 1 ), CreateWindow );
        SetupLevel();

        missileLauncher = Instantiate( missileLauncherPrefab );
        missileLauncher.transform.position = windows.Back().windowCamera.gameObject.transform.position + new Vector3( missileStartPos, 0.0f, 50.0f );
        missileLauncher.transform.localEulerAngles = new Vector3( 0.0f, 0.0f, -90.0f );

        Utility.FunctionTimer.CreateTimer( 2.0f, FireMissile, "FireMissile", true );

        background = Utility.CreateSprite( "Textures/Backgrounds/Repeated 1", desktop.windowCameraStartPosition + new Vector3( 0.0f, 0.0f, 20.0f ), new Vector3( 1.5f, 1.5f ), Quaternion.identity, "SecondaryCamera" );
    }

    void CreateWindow( GameObject shortcut )
    {
        windows.RemoveAll( x => x == null );

        if( windows.Count >= maxWindows )
            return;

        windows.Add( desktop.CreateWindow( "Missiles", false, nextWindowStartPos ).GetComponent<Window>() );
        nextWindowStartPos += new Vector2( 0.3f, nextWindowStartPos.y > -2.0f ? -0.3f : 0.0f );
    }

    protected override void OnLevelUpdate()
    {
        base.OnLevelUpdate();

        foreach( var missile in missiles )
        {
            var found = windows.Any( window =>
            {
                var rect = window.GetCameraViewWorldRect();
                return rect.Contains( missile.missile.GetComponent<Collider2D>().bounds );
            } );

            if( !found )
                Explode( missile.missile, false );
        }
    }

    protected override void OnLevelFinished()
    {
        base.OnLevelFinished();

        missileLauncher.Destroy();
        background.Destroy();

        foreach( var missile in missiles )
            missile.missile.Destroy();

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

        Utility.FunctionTimer.CreateTimer( 0.5f, () =>
        {
            var missile = Instantiate( missilePrefab );
            var spawnLocation = missileLauncher.GetComponentInChildren<Transform>();
            missile.transform.position = spawnLocation.position;
            missile.transform.rotation = spawnLocation.rotation;
            fireLeft = !fireLeft;
            missiles.Add( new Missile()
            {
                missile = missile,
                movement = StartCoroutine( MoveMissileRoutine( missile ) ),
                index = index,
            } );

            Physics2D.SyncTransforms();

            if( fireLeft )
                index++;
        } );

        if( levelCounter == 2 && !fireLeft )
            Utility.FunctionTimer.CreateTimer( 0.75f, FireMissile, "FireMissile2" );
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
        StopCoroutine( missiles.Find( x => x.missile == missile ).movement );

        Utility.FunctionTimer.CreateTimer( 1.0f, () =>
        {
            if( missile != null )
            {
                missiles.RemoveAll( x => x.missile == missile );
                missile.Destroy();
            }
        } );

        var newIndex = missiles.Find( x => x.missile == missile ).index;
        var prevIndex = lastHitIndex;
        lastHitIndex = newIndex;

        if( reached_end )
        {
            if( levelCounter == 2 && prevIndex != newIndex )
                return;

            Utility.FunctionTimer.CreateTimer( 1.0f, () =>
            {
                ++levelCounter;
                SetupLevel();
            } );
        }
    }

    void SetupLevel()
    {
        foreach( var window in windows )
            desktop.DestroyWindow( window );

        windows.Clear();
        nextWindowStartPos = windowStartPos;
        CreateWindow( shortcut );

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

        if( levelCounter > 0 )
            desktop.PlayAudio( stageCompleteAudio );
    }
}