using System;
using UnityEngine;
using UnityEngine.UI;

public class Level11_ButtonMash : BaseLevel
{
    // Static data
    [SerializeField] GameObject window = null;
    [SerializeField] Text text = null;
    [SerializeField] string gameString;
    [SerializeField] int maxKeysEasy = 256;
    [SerializeField] int maxKeysHard = 512;

    // Dynamic data
    int counter = 0;
    UIProgressBar progressBar;

    // Functions
    private void Start()
    {
        window.GetComponent<CanvasGroup>().SetVisibility( false );
    }

    public override void OnStartLevel()
    {
        window.GetComponent<CanvasGroup>().SetVisibility( true );
        progressBar = window.GetComponentInChildren<UIProgressBar>();
    }

    protected override void OnLevelUpdate()
    {
        if( Input.anyKeyDown )
        {
            counter++;
            progressBar.Progress = counter / ( float )( desktop.IsEasyMode() ? maxKeysEasy : maxKeysHard );
            text = 
        }
    }

    protected override void OnLevelFinished()
    {
        window.GetComponent<CanvasGroup>().SetVisibility( false );
    }
}