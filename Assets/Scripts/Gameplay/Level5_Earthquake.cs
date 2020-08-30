using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level5_Earthquake : BaseLevel
{
    // Static data
    [SerializeField] AudioClip quake = null;
    [SerializeField] List<DesktopIcon> data = new List<DesktopIcon>();
    [SerializeField] int numIconsEasy = 5;
    [SerializeField] int numIconsHard = 12;
    [SerializeField] GameObject darknessPrefab = null;
    [SerializeField] float torqueForce = 1.0f;
    [SerializeField] float applyForce = 1.0f;

    // Dynamic data
    int targetsCount, countdown, fails;
    GameObject lightSwitch;
    GameObject taskbarPhysics;
    List<GameObject> shortcuts = new List<GameObject>();
    GameObject darkness = null;
    MeshRenderer darknessMesh = null;
    Vector4[] lightSources = new Vector4[20];
    int lightIndex = 0;
    bool canCreateLights = false;

    public override void OnStartLevel()
    {
        desktop.GetComponent<AudioSource>().PlayOneShot( quake );
        Utility.FunctionTimer.CreateTimer( 1.0f, () => this.ShakeTarget( desktop.GetBackground().transform, 2.0f, 18.0f, 3.0f, 30.0f, 2.0f ) );
        Utility.FunctionTimer.CreateTimer( 2.0f, () => desktop.GetComponent<CanvasGroup>().alpha = 0.0f );
        Utility.FunctionTimer.CreateTimer( 2.1f, () => desktop.GetComponent<CanvasGroup>().alpha = 1.0f );
        Utility.FunctionTimer.CreateTimer( 2.9f, () => desktop.GetComponent<CanvasGroup>().alpha = 0.0f );
        Utility.FunctionTimer.CreateTimer( 2.95f, () => desktop.GetComponent<CanvasGroup>().alpha = 1.0f );
        Utility.FunctionTimer.CreateTimer( 5.0f, () =>
        {
            canCreateLights = true;
            darkness = Instantiate( darknessPrefab );
            darknessMesh = darkness.GetComponent<MeshRenderer>();
            darknessMesh.material.SetFloat( "aspectRatio", darkness.transform.localScale.x / darkness.transform.localScale.y );
        } );

        // Create icons
        for( int i = 0; i < ( desktop.IsEasyMode() ? numIconsEasy : numIconsHard ) && data.Count > 0; ++i )
        {
            var item = i == 0 ? data[0] : data.RandomItem();
            var icon = desktop.CreateShortcut( item, desktop.GetGridBounds().RandomPosition(), ( x ) =>
            {
                if( x == shortcuts[0] )
                    LevelFinished();
            } );

            shortcuts.Add( icon );
            data.Remove( item );
        }

        Utility.FunctionTimer.CreateTimer( 2.0f, () =>
        {
            desktop.TaskbarCreatePhysics();

            foreach( var icon in shortcuts )
            {
                var physics = desktop.ShortcutAddPhysics( icon ).GetComponent<Rigidbody2D>();
                physics.AddTorque( UnityEngine.Random.Range( -torqueForce / 2.0f, torqueForce / 2.0f ) );
                physics.AddForce( UnityEngine.Random.insideUnitCircle * applyForce );
                Utility.FunctionTimer.CreateTimer( 4.0f, () => desktop.ShortcutRemovePhysics( icon ) );
            }

            Utility.FunctionTimer.CreateTimer( 4.0f, () => desktop.TaskbarRemovePhysics() );
        } );
    }

    protected override void OnLevelFinished()
    {
        desktop.TaskbarRemovePhysics();

        foreach( var x in shortcuts )
            desktop.RemoveShortcut( x );

        darkness.Destroy();

        Utility.FunctionTimer.CreateTimer( 3.0f, StartNextLevel );
    }

    protected override void OnLevelUpdate()
    {
        if( canCreateLights )
        {
            if( Input.GetMouseButtonDown( 0 ) )
            {
                // Create light
                float aspectRatio = darkness.transform.localScale.x / darkness.transform.localScale.y;
                var mousePos = desktop.MainCamera.ScreenToViewportPoint( Input.mousePosition );
                var radius = 0.1f;
                lightSources[lightIndex] = new Vector4( mousePos.x, mousePos.y / aspectRatio, radius, 0.0f );
                ++lightIndex;
                darknessMesh.material.SetFloat( "lights", lightIndex );
                darknessMesh.material.SetVectorArray( "lightSources", lightSources );
            }

            //for( int i = 0; i < lightIndex; ++i )
            //    if( lightSources[i].sqrMagnitude > 0.001f )
            //        lightSources[i] = lightSources[i].SetZ( lightSources[i].z + 0.01f * Time.deltaTime );

            //darknessMesh.material.SetFloat( "lights", lightIndex );
            //darknessMesh.material.SetVectorArray( "lightSources", lightSources );
        }
    }
}
