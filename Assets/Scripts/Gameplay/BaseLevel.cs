using UnityEngine;

abstract public class BaseLevel : MonoBehaviour
{
    public Sprite startMenuEntryIcon;
    public string startMenuEntryText;
    public string spoilerTextGameString;
    [SerializeField] AudioClip levelCompleteAudio = null;

    [HideInInspector] public DesktopUIManager desktop;
    protected bool levelStarted;
    protected bool levelActive;

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
        if( !levelStarted )
            return;

        if( levelActive )
            LevelFinished();

        nextLevel?.StartLevel();
    }

    public void LevelFinished( float startNextLevelDelay = 0.0f, bool playCompleteAudio = true )
    {
        if( !levelStarted || !levelActive )
            return;

        if( levelCompleteAudio != null && playCompleteAudio )
            desktop.PlayAudio( levelCompleteAudio );

        desktop.LevelFinished( levelIdx );

        levelActive = false;
        Cleanup( false );
        OnLevelFinished();

        if( startNextLevelDelay > 0.0f )
            Utility.FunctionTimer.CreateTimer( startNextLevelDelay, () => StartNextLevel() );
    }

    public bool HasStarted()
    {
        return levelStarted;
    }

    public void Clear()
    {
        Cleanup( true );
        levelActive = false;
        levelStarted = false;
    }

    public void Restart()
    {
        Cleanup( true );
        levelStarted = false;
        StartLevel();
    }

    private void Update()
    {
        if( levelStarted && levelActive )
        {
            OnLevelUpdate();

            //if( Input.GetKeyDown( KeyCode.Colon ) )
            //    LevelFinished();
        }
    }

    abstract public void OnStartLevel();
    virtual protected void OnLevelUpdate() { }
    virtual protected void Cleanup( bool fromRestart ) { }
    virtual protected void OnLevelFinished() { }
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
}
*/
