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
        desktop.LevelStarted( levelIdx );
        OnStartLevel();
    }

    public void StartNextLevel()
    {
        if( levelActive )
            LevelFinished();

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
    virtual public string GetSpoilerText() { return string.Empty; }
}

/* Template
public class Level9_SOMETHING : BaseLevel
{
    // Static data

    // Dynamic data

    // Functions
    public override void OnStartLevel()
    {
        throw new NotImplementedException();
    }

    protected override void OnLevelUpdate()
    {
        
    }

    protected override void OnLevelFinished()
    {
       
    }

    public override string GetSpoilerText()
    {
        return "";
    }
}
*/
