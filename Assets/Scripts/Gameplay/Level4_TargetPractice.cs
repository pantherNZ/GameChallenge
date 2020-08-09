using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level4_TargetPractice : BaseLevel
{
    [SerializeField] GameObject targetPrefab = null;
    [SerializeField] GameObject gunPrefab = null;
    [SerializeField] float targetSize = 1.0f;
    [SerializeField] float targetSpawnRateBaseSec = 1.0f;
    [SerializeField] float targetSpawnRateVarianceSec = 1.0f;
    [SerializeField] int targetsMax = 30;
    [SerializeField] float targetSpeed = 0.0f;
    [SerializeField] float targetDuration = 4.0f;

    int targetsCount, countdown, score;
    GameObject countdownSprite, gun, crosshair;
    List<GameObject> targets = new List<GameObject>();
    List<GameObject> bullets = new List<GameObject>();

    public override void OnStartLevel()
    {
        GetComponent<CanvasGroup>().SetVisibility( true );
        Cursor.visible = false;

        countdownSprite = Utility.CreateSprite( "Textures/Numbers/spell_rank_3", new Vector3( 0.0f, 0.0f, 20.0f ), new Vector3( 1.0f, 1.0f ) );
        crosshair = Utility.CreateSprite( "Textures/crosshair", desktop.MainCamera.ScreenToWorldPoint( Input.mousePosition ).SetZ( 20.0f ), new Vector3( 1.0f, 1.0f ) );
        gun = Instantiate( gunPrefab, new Vector3( 0.0f, -desktop.GetWorldBound().height / 2.0f, 20.0f ), Quaternion.identity );
        gun.transform.localScale = new Vector3( 1.0f, 1.0f );
        CountDown();
    }

    private void CountDown()
    {
        countdown++;

        if( countdown <= 3 )
        {
            countdownSprite.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>( "Textures/Numbers/spell_rank_" + ( 3 - countdown + 1 ).ToString() );
            Utility.FunctionTimer.CreateTimer( 1.0f, CountDown );
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
        crosshair.transform.position = mousePos;
        mousePos = mousePos.SetY( Mathf.Max( desktop.GetWorldBound().yMin, mousePos.y ) );
        var direction = mousePos - gun.transform.position;
        Vector3 rotatedVectorToTarget = Quaternion.Euler( 0, 0, 90 ) * direction.RotateZ( -90.0f );
        gun.transform.rotation = Quaternion.LookRotation( Vector3.forward, rotatedVectorToTarget );
        
        if( Input.GetMouseButtonDown( 0 ) )
        {
            var barrelEnd = gun.transform.GetChild( 0 ).transform.position;
            direction = mousePos - barrelEnd;
            var bullet = Utility.CreateSprite( "Textures/LineShot/beam_straight", barrelEnd + direction / 2.0f, new Vector3( 1.0f, direction.magnitude ), gun.transform.rotation );
            Utility.FunctionTimer.CreateTimer( 0.1f, () => bullet?.Destroy() );
            bullets.Add( bullet );

            var hit = Physics2D.Raycast( mousePos, Vector2.zero, 0.0f );

            if( hit.collider != null && targets.Contains( hit.collider.gameObject ) )
            {
                hit.collider.gameObject.Destroy();
                score++;
            }
        }
    }

    private float GetSpawnTime()
    {
        return targetSpawnRateBaseSec + UnityEngine.Random.Range( 0.0f, targetSpawnRateVarianceSec );
    }

    private void SpawnTarget()
    {
        ++targetsCount;

        var target = Instantiate( targetPrefab, desktop.GetWorldBound( 1.0f ).RandomPosition().ToVector3( 20.0f ), Quaternion.identity, desktop.MainCamera.transform );
        target.transform.localScale = new Vector3( targetSize, targetSize, 1.0f );
        Utility.FunctionTimer.CreateTimer( targetDuration, () => target?.Destroy() );
        targets.Add( target );

        //if( targetSpeed != 0.0f )


        if( targetsCount < targetsMax )
            Utility.FunctionTimer.CreateTimer( GetSpawnTime(), SpawnTarget );
    }
}
