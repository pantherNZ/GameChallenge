﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level4_TargetPractice : BaseLevel
{
    // Static data
    [SerializeField] GameObject targetPrefab = null;
    [SerializeField] GameObject gunPrefab = null;

    [Serializable]
    public class Data
    {
        public float targetSize = 1.0f;
        public float targetSpawnRateBaseSec = 1.0f;
        public float targetSpawnRateVarianceSec = 1.0f;
        public int targetsMax = 30;
        public float targetDuration = 4.0f;
        public float targetFadeDuration = 2.0f;
    };

    [SerializeField] Data dataEasy = new Data();
    [SerializeField] Data dataHard = new Data();
    [SerializeField] int maxFails = 3;
    [SerializeField] AudioClip shootAudio = null;
    [SerializeField] AudioClip hitAudio = null;
    [SerializeField] Color crosshairColour = Color.white;
    [SerializeField] GameObject missTextPrefab = null;

    // Dynamic data
    int targetsCount, countdown, fails;
    GameObject countdownSprite, gun, crosshair;
    List<GameObject> targets = new List<GameObject>();
    List<GameObject> bullets = new List<GameObject>();

    GameObject flag;
    int flagSpawnIdx;

    private void Start()
    {
        var pos = desktop.GetScreenBound( 100.0f, false ).RandomPosition();// + ( desktop.transform as RectTransform ).rect.size / 2.0f;
        flag = desktop.CreateFlag( pos, 4, true, true, "4", true );
        flagSpawnIdx = 1;//UnityEngine.Random.Range( 0, GetData().targetsMax ); //  

        Utility.FunctionTimer.CreateTimer( 1.0f, () =>
        {
            if( flag != null )
                ( flag.transform as RectTransform ).localPosition = desktop.GetScreenBound( 100.0f, false ).RandomPosition();
        }, "Dfg", true );
    }

    public override void OnStartLevel()
    {
        GetComponent<CanvasGroup>().SetVisibility( true );
        Cursor.visible = false;
        desktop.desktopSelectionEnabled = false;

        targetsCount = countdown = fails = 0;
        
        countdownSprite = Utility.CreateSprite( "Textures/Numbers/spell_rank_3", new Vector3( 0.0f, 0.0f, 20.0f ), new Vector3( 1.0f, 1.0f ) );
        crosshair = Utility.CreateSprite( "Textures/ShootingLevel/crosshair", desktop.MainCamera.ScreenToWorldPoint( Input.mousePosition ).SetZ( 20.0f ), new Vector3( 1.0f, 1.0f ) );
        crosshair.GetComponent<SpriteRenderer>().color = crosshairColour;
        gun = Instantiate( gunPrefab, new Vector3( 0.0f, -desktop.GetWorldBound().height / 2.0f, 20.0f ), Quaternion.identity );
        gun.transform.localScale = new Vector3( 1.0f, 1.0f );
        CountDown();
    }

    void CheckLevelComplete()
    {
        targets.RemoveAll( x => x == null );

        // Failed
        if( fails >= maxFails )
        {
            desktop.LevelFailed();
        }
        // Success
        else if( targetsCount >= GetData().targetsMax && targets.IsEmpty() )
        {
            LevelFinished( 3.0f );
            return;
        }
        else
        {
            return;
        }

        LevelFinished( 0.0f, false );
    }

    protected override void Cleanup( bool fromRestart )
    {
        bullets.DestroyAll();
        targets.DestroyAll();
        countdownSprite?.Destroy();
        gun?.Destroy();
        crosshair?.Destroy();
        targetsCount = countdown = fails = 0;
        Cursor.visible = true;
        Utility.FunctionTimer.StopTimer( "CountDown" );
        Utility.FunctionTimer.StopTimer( "SpawnTarget" );
    }

    private void CountDown()
    {
        countdown++;

        if( countdown <= 3 )
        {
            countdownSprite.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>( "Textures/Numbers/spell_rank_" + ( 3 - countdown + 1 ).ToString() );
            Utility.FunctionTimer.CreateTimer( 1.0f, CountDown, "CountDown" );
        }
        else
        {
            countdownSprite.Destroy();
            SpawnTarget();
        }
    }

    protected override void OnLevelUpdate()
    {
        var mousePos = desktop.MainCamera.ScreenToWorldPoint( Input.mousePosition ).SetZ( 20.0f );
        crosshair.transform.position = mousePos.SetZ( 1.0f );
        mousePos = mousePos.SetY( Mathf.Max( desktop.GetWorldBound().yMin, mousePos.y ) );
        var direction = mousePos - gun.transform.position;
        Vector3 rotatedVectorToTarget = Quaternion.Euler( 0, 0, 90 ) * direction.RotateZ( -90.0f );
        gun.transform.rotation = Quaternion.LookRotation( Vector3.forward, rotatedVectorToTarget );
        gun.transform.localScale = gun.transform.localScale.SetX( Mathf.Abs( gun.transform.localScale.x ) * ( gun.transform.rotation.eulerAngles.z >= 180.0f ? 1.0f : -1.0f ) );

        if( Input.GetMouseButtonDown( 0 ) && !desktop.ContextMenuVisibile() )
        {
            var barrelEnd = gun.transform.GetChild( 0 ).transform.position;
            direction = mousePos - barrelEnd;
            var bullet = Utility.CreateSprite( "Textures/LineShot/beam_straight", barrelEnd + direction / 2.0f, new Vector3( 1.0f, direction.magnitude ), gun.transform.rotation );
            Utility.FunctionTimer.CreateTimer( 0.07f, () => bullet?.Destroy() );
            bullets.Add( bullet );
            desktop.PlayAudio( shootAudio );

            var hit = Physics2D.Raycast( mousePos, Vector2.zero, 0.0f );

            if( !targets.IsEmpty() )
            {
                if( hit.collider != null && targets.Contains( hit.collider.gameObject ) )
                {
                    targets.Remove( hit.collider.gameObject );
                    hit.collider.gameObject.Destroy();
                    desktop.PlayAudio( hitAudio );
                    CheckLevelComplete();
                }
                else if( !desktop.IsEasyMode() )
                { 
                    Miss( mousePos );
                }
            }
        }
    }

    private Data GetData()
    {
        return desktop.IsEasyMode() ? dataEasy : dataHard;
    }

    private float GetSpawnTime()
    {
        return GetData().targetSpawnRateBaseSec + UnityEngine.Random.Range( 0.0f, GetData().targetSpawnRateVarianceSec );
    }

    private void SpawnTarget()
    {
        ++targetsCount;

        if( targetsCount == flagSpawnIdx && flag != null )
        {
            flag.GetComponent<CanvasGroup>().SetVisibility( true );
            SetupTarget( flag );
        }

        Vector3 position;

        do
        {
            position = desktop.GetWorldBound( 1.0f ).RandomPosition().ToVector3( 20.0f );
        }
        while( ( position - gun.transform.position ).sqrMagnitude <= 6.0f * 6.0f );

        var target = Instantiate( targetPrefab, position, Quaternion.identity, desktop.MainCamera.transform );
        SetupTarget( target );
        targets.Add( target );

        if( targetsCount < GetData().targetsMax )
            Utility.FunctionTimer.CreateTimer( GetSpawnTime(), SpawnTarget, "SpawnTarget" );
    }

    private void SetupTarget( GameObject target )
    {
        target.transform.localScale = new Vector3( 0.1f, 0.1f, 0.1f );
        float targetFadeDuration = GetData().targetFadeDuration;
        float targetDuration = GetData().targetDuration;
        float targetSize = GetData().targetSize;

        StartCoroutine( Utility.InterpolateScale( target.transform, new Vector3( targetSize, targetSize, 1.0f ), targetFadeDuration ) );

        Utility.FunctionTimer.CreateTimer( targetFadeDuration + targetDuration, () =>
        {
            if( target != null )
            {
                StartCoroutine( Utility.InterpolateScale( target.transform, new Vector3(), targetFadeDuration ) );
                Utility.FunctionTimer.CreateTimer( targetFadeDuration, () =>
                {
                    if( target != null )
                    {
                        target.Destroy();
                        if( target != flag )
                            Miss( target.transform.position );
                    }
                } );
            }
        } );
    }

    private void Miss( Vector3 position )
    {
        fails++;
        CheckLevelComplete();

        var obj = Instantiate( missTextPrefab, desktop.DesktopCanvas );
        obj.GetComponent<Text>().color = Color.black;
        obj.transform.position = position;
        this.FadeToColour( obj.GetComponent<Text>(), Color.black.SetA( 0.0f ), 1.0f );
        this.InterpolatePosition( obj.transform, obj.transform.position + new Vector3( 0.0f, 1.0f, 0.0f ), 1.0f );
        Utility.FunctionTimer.CreateTimer( 1.0f, () => obj.Destroy() );
    }
}
