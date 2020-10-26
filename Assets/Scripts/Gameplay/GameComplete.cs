using System;
using UnityEngine;

public class GameComplete : BaseLevel
{
    [SerializeField] GameObject completeCamera = null;

    void Start()
    {
        completeCamera.gameObject.SetActive( false );
    }

    public override void OnStartLevel()
    {
        completeCamera.gameObject.SetActive( true );
        desktop.MainCamera.gameObject.SetActive( false );
    }

    protected override void OnLevelFinished()
    {
        base.OnLevelFinished();
    }

    protected override void OnLevelUpdate()
    {
        base.OnLevelUpdate();
    }
}
