using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Level6_Missile : BaseLevel
{
    [SerializeField] GameObject missilePrefab = null;
    [SerializeField] GameObject stage1Prefab = null;
    [SerializeField] GameObject stage2Prefab = null;
    [SerializeField] GameObject stage3Prefab = null;
    [SerializeField] float moveSpeed = 30.0f;
    [SerializeField] float moveSpeed2 = 10.0f;
    [SerializeField] float explosionRadius = 5.0f;
    [SerializeField] float explosionStrength = 5.0f;
    [SerializeField] Vector2 windowStartPos = new Vector2( -3.0f, 0.0f );
    Vector2 nextWindowStartPos;
    [SerializeField] AudioClip fireAudio = null;
    [SerializeField] AudioClip explodeAudio = null;
    [SerializeField] AudioClip stageCompleteAudio = null;
    [SerializeField] string finalStageHintGameString = string.Empty;
    List<Window> windows = new List<Window>();
    GameObject shortcut, missileLauncher, missileLauncher2, level;
    List<GameObject> paths;

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
    int hitCount = 0;

    public override void OnStartLevel()
    {
        maxWindows = desktop.IsEasyMode() ? 4 : 2;

        var icon = new DesktopIcon()
        {
            name = "Missiles",
            icon = startMenuEntryIcon
        };

        levelCounter = 0;
        shortcut = desktop.CreateShortcut( icon, new Vector2Int( 0, 1 ), CreateWindow );
        SetupLevel();

        Utility.FunctionTimer.CreateTimer( 4.0f, FireMissile, "FireMissile", true );
    }

    void CreateWindow( GameObject shortcut )
    {
        if( windows.Count >= maxWindows )
            return;

        windows.Add( desktop.CreateWindow( "Missiles", false, nextWindowStartPos ).GetComponent<Window>() );
        nextWindowStartPos += new Vector2( 30.0f, nextWindowStartPos.y > -desktop.MainCamera.pixelHeight / 2.0f + 250.0f ? -30.0f : 0.0f );
    }

    protected override void OnLevelUpdate()
    {
        base.OnLevelUpdate();

        windows.RemoveAll( x => x == null );

        if( windows.IsEmpty() )
        {
            while( !missiles.IsEmpty() )
                Explode( missiles.Front().missile, false, false );
        }
        else
        {
            foreach( var missile in missiles )
            {
                var found = windows.Any( window =>
                {
                    var rect = window.GetCameraViewWorldRect();
                    return rect.Contains( missile.missile.GetComponent<Collider2D>().bounds );
                } );

                if( !found )
                {
                    Explode( missile.missile, false );
                    break;
                }
            }
        }

        /*if( level != null )
        {
            var width = desktop.MainCamera.pixelWidth / 1400.0f;
            var height = desktop.MainCamera.pixelHeight / 600.0f;
            //level.transform.localScale = new Vector3( width, height, 1.0f );

            foreach( var path in paths )
                path.gameObject.transform.localScale = new Vector3( 1.0f / width, 1.0f / height, 1.0f );
        }*/
    }

    protected override void Cleanup( bool fromRestart )
    {
        level.Destroy();

        foreach( var missile in missiles )
            missile.missile.Destroy();

        foreach( var window in windows)
            desktop.DestroyWindow( window );

        windows.Clear();
        missiles.Clear();

        Utility.FunctionTimer.StopTimer( "FireMissile" );
        Utility.FunctionTimer.StopTimer( "FireMissile2" );

        desktop.RemoveShortcut( shortcut );
    }

    void FireMissile()
    {
        var launcher = ( fireLeft && missileLauncher2 != null ) ? missileLauncher2 : missileLauncher;

        var found = windows.Any( window =>
        {
            var rect = window.GetCameraViewWorldRect();
            return rect.Contains( launcher.transform.position );
        } );

        if( !found )
            return;

        launcher.GetComponent<Animator>().Play( "Fire" );
        desktop.PlayAudio( fireAudio, 0.4f );

        bool fireLeftLocal = fireLeft;

        Utility.FunctionTimer.CreateTimer( 0.5f, () =>
        {
            var missile = Instantiate( missilePrefab );
            var spawnLocation = launcher.GetComponentInChildren<Transform>();
            missile.transform.position = spawnLocation.position;
            missile.transform.rotation = spawnLocation.rotation;
            missiles.Add( new Missile()
            {
                missile = missile,
                movement = StartCoroutine( MoveMissileRoutine( missile, fireLeftLocal ) ),
                index = index,
            } );

            Physics2D.SyncTransforms();
        } );

        if( !fireLeft )
            index++;

        fireLeft = !fireLeft;

        if( levelCounter == 2 && fireLeft )
            FireMissile();
    }

    public IEnumerator MoveMissileRoutine( GameObject missile, bool fireLeftLocal )
    {
        var paths = level.GetComponentsInChildren<PathCreation.PathCreator>();

        switch( levelCounter )
        {
            case 0:
                yield return Utility.InterpolateAlongPath( missile.transform, paths[0], paths[0].path.length / moveSpeed );
                break;
            case 1:
                yield return Utility.InterpolateAlongPath( missile.transform, paths[0], paths[0].path.length / moveSpeed2 );
                break;
            case 2:
            {
                if( fireLeftLocal )
                    yield return Utility.InterpolateAlongPath( missile.transform, paths[0], paths[0].path.length / moveSpeed2 );
                else
                    yield return Utility.InterpolateAlongPath( missile.transform, paths[1], paths[1].path.length / moveSpeed2 );
                break;
            }
        }

        Explode( missile, true );
    }

    public void Explode( GameObject missile, bool reached_end, bool playEffects = true )
    {
        if( missile == null )
            return;

        if( playEffects )
        {
            missile.GetComponent<Animator>().Play( "Explode" );
            desktop.PlayAudio( explodeAudio );
        }

        var found = missiles.Find( x => x.missile == missile );
        StopCoroutine( found.movement );

        Utility.FunctionTimer.CreateTimer( 1.0f, () =>
        {
            if( missile != null )
                missile.Destroy();
        } );

        var newIndex = found.index;
        missiles.Remove( found );

        if( reached_end )
        {
            hitCount++;

            Utility.FunctionTimer.CreateTimer( 0.25f, () => { hitCount = 0; } );

            if( levelCounter == 2 && hitCount < 2 )
                return;

            var overlaps = Physics2D.OverlapCircleAll( missile.transform.position, explosionRadius );

            foreach( var overlap in overlaps )
            {
                if( overlap.gameObject != missile && overlap.attachedRigidbody != null )
                {
                    overlap.attachedRigidbody.bodyType = RigidbodyType2D.Dynamic;
                    overlap.attachedRigidbody.AddForce( ( overlap.transform.position - missile.transform.position ).normalized * explosionStrength );
                }
            }

            Utility.FunctionTimer.CreateTimer( 1.5f, () =>
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
                desktop.ResetSpoiler( finalStageHintGameString );
                break;
            case 3:
            {
                LevelFinished( 1.0f );
                SubtitlesManager.Instance.AddSubtitleGameString( "Narrator_Level_6_Complete" );
                break;
            }
        }

        level?.Destroy();
        level = Instantiate( levelCounter == 0 ? stage1Prefab : levelCounter == 1 ? stage2Prefab : stage3Prefab );
        level.transform.position = desktop.windowCameraStartPosition.SetZ( 10.0f );
        missileLauncher = level.transform.GetChild( 0 ).gameObject;

        if( levelCounter == 2 )
            missileLauncher2 = level.transform.GetChild( 1 ).gameObject;

        paths = level.GetComponentsInChildren<PathCreation.PathCreator>().Select( x => x.gameObject ).ToList();

        if( levelCounter > 0 )
            desktop.PlayAudio( stageCompleteAudio );
    }
}