using UnityEngine;

abstract public class BaseLevel : MonoBehaviour
{
    protected DesktopUIManager desktop;
    bool levelStarted;

    private void Start()
    {
        desktop = GetComponent<DesktopUIManager>();
    }

    public void StartLevel()
    {
        OnStartLevel();
        levelStarted = true;
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