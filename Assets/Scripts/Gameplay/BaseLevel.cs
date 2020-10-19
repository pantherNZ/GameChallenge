using UnityEngine;

abstract public class BaseLevel : MonoBehaviour
{
    [HideInInspector] public DesktopUIManager desktop;
    bool levelStarted;
    bool levelActive;

    [HideInInspector]
    public int levelIdx = 0;

    [HideInInspector]
    public BaseLevel nextLevel;

    private void Start()
    {

    }

    public void StartLevel()
    {
        if( levelStarted )
            return;

        SubtitlesManager.Instance.ClearSubtitles();
        levelStarted = true;
        levelActive = true;
        OnStartLevel();
    }

    public void StartNextLevel()
    {
        nextLevel?.StartLevel();
    }

    public void LevelFinished()
    {
        if( !levelStarted || !levelActive )
            return;

        levelActive = false;
        OnLevelFinished();
    }

    public bool HasStarted()
    {
        return levelStarted;
    }

    private void Update()
    {
        if( levelStarted && levelActive )
            OnLevelUpdate();
    }

    abstract public void OnStartLevel();
    virtual protected void OnLevelUpdate() { }
    virtual protected void OnLevelFinished() { }
}