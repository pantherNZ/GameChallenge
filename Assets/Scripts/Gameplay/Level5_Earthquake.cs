﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level5_Earthquake : BaseLevel
{
    // Static data
    [SerializeField] AudioClip quakeAudio = null;
    [SerializeField] List<DesktopIcon> data = new List<DesktopIcon>();
    [SerializeField] int numIconsEasy = 5;
    [SerializeField] int numIconsHard = 12;
    [SerializeField] GameObject darknessPrefab = null;
    [SerializeField] float torqueForce = 1.0f;
    [SerializeField] float applyForce = 1.0f;
    [SerializeField] float lightCooldownSec = 0.3f;
    [SerializeField] float lightRadius = 0.1f;
    [SerializeField] float lightFadeSpeedSec = 0.4f;

    // Dynamic data
    List<GameObject> shortcuts = new List<GameObject>();
    GameObject darkness = null;
    MeshRenderer darknessMesh = null;
    Vector4[] lightSources = new Vector4[10];
    int lightIndex = 0;
    bool canCreateLights = false;
    float cooldown = 0.0f;
    EventDispatcher generator;
    GameObject flag;

    private void Start()
    {
        flag = desktop.CreateFlag( new Vector2( desktop.GetScreenBound().width / 2.0f - 100.0f, desktop.GetScreenBound().height / 2.0f - 200.0f ), 5, true, true, "5" );
    }

    public override void OnStartLevel()
    {
        desktop.PlayAudio( quakeAudio );
        float timer = 1.0f;
        Utility.FunctionTimer.CreateTimer( timer, () => this.Shake( desktop.GetBackground().transform, 2.0f, 18.0f, 3.0f, 30.0f, 2.0f ) ); timer += 0.5f;
        Utility.FunctionTimer.CreateTimer( timer, () => desktop.GetComponent<CanvasGroup>().alpha = 0.0f ); timer += 0.1f;
        Utility.FunctionTimer.CreateTimer( timer, () => desktop.GetComponent<CanvasGroup>().alpha = 1.0f ); timer += 0.8f;
        Utility.FunctionTimer.CreateTimer( timer, () => desktop.GetComponent<CanvasGroup>().alpha = 0.0f ); timer += 0.1f;
        Utility.FunctionTimer.CreateTimer( timer, () => desktop.GetComponent<CanvasGroup>().alpha = 1.0f ); timer += 0.6f;
        Utility.FunctionTimer.CreateTimer( timer, () => desktop.GetComponent<CanvasGroup>().alpha = 0.0f ); timer += 0.1f;
        Utility.FunctionTimer.CreateTimer( timer, () => desktop.GetComponent<CanvasGroup>().alpha = 1.0f ); timer += 0.5f;
        Utility.FunctionTimer.CreateTimer( timer, () => desktop.GetComponent<CanvasGroup>().alpha = 0.0f ); timer += 0.1f;
        Utility.FunctionTimer.CreateTimer( timer, () => desktop.GetComponent<CanvasGroup>().alpha = 1.0f ); timer += 0.9f;
        Utility.FunctionTimer.CreateTimer( timer, () =>
        {
            canCreateLights = true;
            desktop.contextMenuEnabled = false;
            darkness = Instantiate( darknessPrefab );
            darknessMesh = darkness.GetComponent<MeshRenderer>();
            darknessMesh.material.SetFloat( "aspectRatio", darkness.transform.localScale.x / darkness.transform.localScale.y );
            flag.GetComponent<CanvasGroup>().SetVisibility( true );
        } );

        // Create icons
        for( int i = 0; i < ( desktop.IsEasyMode() ? numIconsEasy : numIconsHard ) && data.Count > 0; ++i )
        {
            var item = i == 0 ? data[0] : data.RandomItem();
            var icon = desktop.CreateShortcut( item, desktop.GetGridBounds().RandomPosition(), ( x ) =>
            {
                if( canCreateLights && x == shortcuts[0] )
                    LevelFinished( 2.0f );
            } );

            shortcuts.Add( icon );
            data.Remove( item );
        }

        generator = shortcuts[0].GetComponent<EventDispatcher>();

        Utility.FunctionTimer.CreateTimer( 2.0f, () =>
        {
            desktop.CreatePhysicsBound();

            foreach( var icon in shortcuts )
                if( icon != null )
                    CreateShortcutPhysics( icon );

            CreateShortcutPhysics( desktop.shortcuts[0].shortcut );
            Utility.FunctionTimer.CreateTimer( 4.0f, () =>
            {
                desktop.ShortcutRemovePhysics( desktop.shortcuts[0].shortcut, true );
                desktop.shortcuts[0].shortcut.transform.localRotation = Quaternion.identity;
             } );
        } );
    }

    private void CreateShortcutPhysics( GameObject shortcut )
    {
        var physics = desktop.ShortcutAddPhysics( shortcut ).GetComponent<Rigidbody2D>();
        var eventDispatcher = shortcut.AddComponent<EventDispatcher>();
        eventDispatcher.enabled = false;
        physics.AddTorque( UnityEngine.Random.Range( -torqueForce / 2.0f, torqueForce / 2.0f ) );
        physics.AddForce( UnityEngine.Random.insideUnitCircle * applyForce );
    }

    protected override void Cleanup( bool fromRestart )
    {
        desktop.RemovePhysicsBound();

        foreach( var x in shortcuts )
            desktop.RemoveShortcut( x );

        darkness.Destroy();
        desktop.contextMenuEnabled = true;

        if( flag != null )
            flag.GetComponent<CanvasGroup>().SetVisibility( false );
    }

    protected override void OnLevelFinished()
    {

    }

    protected override void OnLevelUpdate()
    {
        if( darkness != null )
        {
            var desktopBound = desktop.GetWorldBound( 0.0f, true );
            darkness.transform.localScale = new Vector3( desktopBound.width, desktopBound.height, 1.0f );
        }

        if( canCreateLights )
        {
            if( cooldown <= 0.0f )
            {
                if( Input.GetMouseButtonDown( 1 ) )
                {
                    // Create light
                    float aspectRatio = darkness.transform.localScale.x / darkness.transform.localScale.y;
                    var mousePos = desktop.MainCamera.ScreenToViewportPoint( Input.mousePosition );
                    lightSources[lightIndex] = new Vector4( mousePos.x, mousePos.y / aspectRatio, lightRadius, 0.0f );
                    darknessMesh.material.SetVectorArray( "lightSources", lightSources );
                    cooldown = lightCooldownSec;
                    lightIndex = ( lightIndex + 1 ) % lightSources.Length;
                }
            }
            else
            {
                cooldown -= Time.deltaTime;
            }

            if( darknessMesh != null )
            {
                //generator.enabled = false;
                generator.enabled = true;

                for( int i = lightSources.Length - 1; i >= 0; --i )
                {
                    if( lightSources[i].sqrMagnitude > 0.001f )
                    {
                        float interp = lightSources[i].z >= lightRadius - 0.01f ? 0.05f : lightFadeSpeedSec;
                        lightSources[i] = lightSources[i].SetZ( Mathf.Max( 0.0f, lightSources[i].z - interp * Time.deltaTime ) );

                        var screenPos = desktop.MainCamera.ScreenToViewportPoint( ( generator.transform as RectTransform ).localPosition );
                        var direction = lightSources[i].ToVector2() - ( screenPos.ToVector2() + new Vector2( 0.5f, 0.5f ) );
                        //generator.enabled |= direction.SqrMagnitude() <= lightSources[i].z * lightSources[i].z;
                    }
                }

                darknessMesh.material.SetVectorArray( "lightSources", lightSources );
            }
        }
    }
}
