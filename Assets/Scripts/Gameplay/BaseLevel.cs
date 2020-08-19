using UnityEngine;

abstract public class BaseLevel : MonoBehaviour
{
    protected DesktopUIManager desktop;
    bool levelStarted;

    [HideInInspector]
    public int levelIdx = 0;

    [HideInInspector]
    public BaseLevel nextLevel;

    private void Start()
    {
        desktop = GetComponent<DesktopUIManager>();
    }

    public void StartLevel()
    {
        if( levelStarted )
            return;

        levelStarted = true;
        OnStartLevel();
    }

    public void StartNextLevel()
    {
        nextLevel?.StartLevel();
    }

    public bool HasStarted()
    {
        return levelStarted;
    }

    private void Update()
    {
        if( levelStarted )
            OnLevelUpdate();
    }

    abstract public void OnStartLevel();
    virtual protected void OnLevelUpdate() { }
}