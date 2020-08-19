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

    // Dynamic data
    int targetsCount, countdown, fails;
    GameObject lightSwitch;
    GameObject taskbarPhysics;
    List<Pair<GameObject, GameObject>> shortcuts = new List<Pair<GameObject, GameObject>>();

    Vector3 rootPos = new Vector3( -20.0f, 0.0f, 0.0f );

    public override void OnStartLevel()
    {
        desktop.GetComponent<AudioSource>().PlayOneShot( quake );
        Utility.FunctionTimer.CreateTimer( 1.0f, () => this.ShakeTarget( desktop.GetBackground().transform, 2.0f, 18.0f, 3.0f, 30.0f, 2.0f ) );
        Utility.FunctionTimer.CreateTimer( 2.0f, () => desktop.GetComponent<CanvasGroup>().alpha = 0.0f );
        Utility.FunctionTimer.CreateTimer( 2.1f, () => desktop.GetComponent<CanvasGroup>().alpha = 1.0f );
        Utility.FunctionTimer.CreateTimer( 2.9f, () => desktop.GetComponent<CanvasGroup>().alpha = 0.0f );
        Utility.FunctionTimer.CreateTimer( 2.95f, () => desktop.GetComponent<CanvasGroup>().alpha = 1.0f );
        //Utility.FunctionTimer.CreateTimer( 5.0f, () => desktop.GetComponent<CanvasGroup>().alpha = 0.0f );

        var taskbarPhysics = Utility.CreateWorldObjectFromScreenSpaceRect( desktop.Taskbar.transform as RectTransform );
        taskbarPhysics.transform.position = taskbarPhysics.transform.position + rootPos;
        taskbarPhysics.AddComponent<Quad>();
        taskbarPhysics.AddComponent<BoxCollider2D>().size = new Vector2( 1.0f, 1.0f );

        for( int i = 0; i < ( desktop.IsEasyMode() ? numIconsEasy : numIconsHard ) && data.Count > 0; ++i )
        {
            var item = data.RandomItem();
            var icon = desktop.CreateShortcut( item, desktop.GetGridBounds().RandomPosition() );
            icon.GetComponent<LockToGrid>().enabled = false;
            data.Remove( item );

            var physics = Utility.CreateWorldObjectFromScreenSpaceRect( icon.transform as RectTransform );
            var test = ( icon.transform as RectTransform ).GetWorldRect();

            physics.transform.position = physics.transform.position + rootPos;
            physics.AddComponent<Quad>();
            physics.AddComponent<BoxCollider2D>().size = new Vector2( 1.0f, 1.0f );
            physics.AddComponent<Rigidbody2D>();

            shortcuts.Add( new Pair<GameObject, GameObject>( icon, physics ) );
        }
    }

    private void Update()
    {
        if( Input.GetMouseButtonDown( 0 ) )
        {
            // Create light
        }
    }

    private void LateUpdate()
    {
        foreach( var shortcut in shortcuts )
        {
            var newPos = shortcut.Second.transform.position - rootPos;// + shortcut.Second.transform.localScale / 2.0f;
            ( shortcut.First.transform as RectTransform ).localPosition = ( desktop.transform as RectTransform ).InverseTransformPoint( newPos ).SetZ( 0.0f );
            ( shortcut.First.transform as RectTransform ).localRotation = shortcut.Second.transform.rotation;
        }
    }
}
