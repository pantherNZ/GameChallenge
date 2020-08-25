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
    [SerializeField] GameObject darkness = null;

    // Dynamic data
    int targetsCount, countdown, fails;
    GameObject lightSwitch;
    GameObject taskbarPhysics;
    List<GameObject> shortcuts = new List<GameObject>();

    public override void OnStartLevel()
    {
        desktop.GetComponent<AudioSource>().PlayOneShot( quake );
        Utility.FunctionTimer.CreateTimer( 1.0f, () => this.ShakeTarget( desktop.GetBackground().transform, 2.0f, 18.0f, 3.0f, 30.0f, 2.0f ) );
        Utility.FunctionTimer.CreateTimer( 2.0f, () => desktop.GetComponent<CanvasGroup>().alpha = 0.0f );
        Utility.FunctionTimer.CreateTimer( 2.1f, () => desktop.GetComponent<CanvasGroup>().alpha = 1.0f );
        Utility.FunctionTimer.CreateTimer( 2.9f, () => desktop.GetComponent<CanvasGroup>().alpha = 0.0f );
        Utility.FunctionTimer.CreateTimer( 2.95f, () => desktop.GetComponent<CanvasGroup>().alpha = 1.0f );
        //Utility.FunctionTimer.CreateTimer( 5.0f, () => desktop.GetComponent<CanvasGroup>().alpha = 0.0f );

        desktop.TaskbarCreatePhysics();

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
            desktop.ShortcutAddPhysics( icon );
            Utility.FunctionTimer.CreateTimer( 4.0f, () => desktop.ShortcutRemovePhysics( icon ) );
        }
    }

    protected override void OnLevelFinished()
    {
        desktop.TaskbarRemovePhysics();

        foreach( var x in shortcuts )
            desktop.RemoveShortcut( x );

        darkness.Destroy();

        Utility.FunctionTimer.CreateTimer( 3.0f, StartNextLevel );
    }

    private void Update()
    {
        if( Input.GetMouseButtonDown( 0 ) )
        {
            // Create light
        }
    }
}
