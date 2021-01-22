﻿using System;
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
    List<Window> windows = new List<Window>();
    GameObject shortcut, missileLauncher, level;

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

        Utility.FunctionTimer.CreateTimer( 2.0f, FireMissile, "FireMissile", true );
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

        level.Destroy();

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
                if( fireLeft )
                    yield return Utility.InterpolateAlongPath( missile.transform, paths[0], paths[0].path.length / moveSpeed2 );
                else
                    yield return Utility.InterpolateAlongPath( missile.transform, paths[1], paths[1].path.length / moveSpeed2 );
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

            var overlaps = Physics2D.OverlapCircleAll( missile.transform.position, explosionRadius );

            foreach( var overlap in overlaps )
                if( overlap.gameObject != missile && overlap.attachedRigidbody != null )
                    overlap.attachedRigidbody.AddRelativeForce( ( overlap.transform.position - missile.transform.position ).normalized * explosionStrength );

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

        level?.Destroy();
        level = Instantiate( levelCounter == 0 ? stage1Prefab : levelCounter == 1 ? stage2Prefab : stage3Prefab );
        level.transform.position = windows.Back().windowCamera.gameObject.transform.position.SetZ( 10.0f );
        missileLauncher = level.transform.GetChild( 0 ).gameObject;

        if( levelCounter > 0 )
            desktop.PlayAudio( stageCompleteAudio );
    }
}