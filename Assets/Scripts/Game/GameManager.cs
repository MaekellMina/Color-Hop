using UnityEngine;
using System.Collections;

public enum HopColor
{
    RED,
    BLUE,
    YELLOW
}

public class Constants
{
    //hop targets spawnable bounds
    public const float MAX_SPAWNABLE_X = 2f;
    public const float MIN_SPAWNABLE_X = -2f;
    public const float MAX_SPAWNABLE_Y = 2.5f;
    public const float MIN_SPAWNABLE_Y = -1.5f;

    public const int NUMBER_OF_HOPCOLORS = 3;
}


public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public string Path;                     // Path to where the game data will be saved

    private bool b_pass = false;            // Did the game succeed or fail?
    private int gameScore = 0;

    internal static GameManager instance;   // singleton instance
    //Put your game states here
    public enum GAMESTATES
    {
        INIT,
        INGAME,
        PAUSED,
        GAMEOVER
    }

    public GAMESTATES gameState = GAMESTATES.INIT;

    private bool b_gameover;                // Is the game over?

    bool callOnce = true;                   // Used when changing the game state bool for calling function/code once in the game

    //--------public game fields
    public GameObject player;               // reference to player token AKA Hopper
    public GameObject basicHopTargetPrefab; // prefab for a basic hop target
    public Color redAppearance;
    public Color blueAppearance;
    public Color yellowAppearance;
    public HUD gameHUD;                     // game HUD
    public AnimationCurve hopperScaleAnimationCurve;

    //--------private game fields
    private Vector2 restPos;                // idle position every after hop, also start position
    private HopTarget curHopTarget;         // target to hop on to 
    private HopColor hopperColor;           // current color of the player
    private SpriteRenderer hopperSprite;    // reference to the sprite renderer of player
    private Coroutine hopCoroutine;         //reference to hop coroutine

    void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        //Initialize class
        instance = this;

    }

    void Start()
    {
        SETPATH();

        // Do necessary initialization here
        // put here the initializations that should not be called when game resets (WE DO NOT RELOAD SCENE WHEN RESETTING GAME)

        restPos = player.transform.position;
        hopperSprite = player.GetComponent<SpriteRenderer>();

        //create hop targets pool
        for(int i = 0; i < 10; i++)     // 10 basic hop targets
        {
            CacheManager.Store("Basic_HopTarget", Instantiate(basicHopTargetPrefab) as GameObject);
        }

        //for these to work, EventManager.cs must be on the hierarchy
        EventsManager.OnGameReset.AddListener(OnGameReset);
        EventsManager.OnGamePaused.AddListener(OnGamePaused);
        EventsManager.OnGameOver.AddListener(OnGameOver);
    }

    void OnGameReset()
    {

    }
    void OnGamePaused()
    {

    }
    void OnGameOver()
    {

    }

    #region FSM
    void Update()
    {
        GameFSM();
    }
    void OnEnable()
    {

    }
    void OnDisable()
    {
        EventsManager.OnGameReset.RemoveListener(OnGameReset);
        EventsManager.OnGamePaused.RemoveListener(OnGamePaused);
        EventsManager.OnGameOver.RemoveListener(OnGameOver);
    }
    void GameFSM()
    {
        switch(gameState)
        {
            case GAMESTATES.INIT:
                if(callOnce)
                {
                    // -- Put codes that are needed to be called only once -- //
                    //Do the setup for the game here.

                    gameScore = 0;
                    player.transform.position = restPos;
                    
                    //
                    callOnce = false;
                    //change gamestate after running init once
                    ChangeGameState(GAMESTATES.INGAME);
                }
                break;
            case GAMESTATES.INGAME:
                if (callOnce)
                {
                    // -- Put codes that are needed to be called only once -- //

                    //
                    callOnce = false;
                }
                //Game Loop
                Game();
                break;
            case GAMESTATES.PAUSED:
                if (callOnce)
                {
                    // -- Put codes that are needed to be called only once -- //

                    EventsManager.OnGamePaused.Invoke();

                    //
                    callOnce = false;
                }

                break;
            case GAMESTATES.GAMEOVER:
                if (callOnce)
                {
                    // -- Put codes that are needed to be called only once -- //
                    b_gameover = true;

                    StartCoroutine(GameOver());
                    //
                    callOnce = false;
                }
                break;

        }
    }
    public void ChangeGameState(int state)  //for button click event (just in case)
    {
        gameState = (GAMESTATES)state;
        callOnce = true;
    }
    public void ChangeGameState(GAMESTATES state)
    {
        gameState = state;
        callOnce = true;        // Set to true so every time the state change, there's a place to call some code once in the loop
    }
    #endregion

    IEnumerator GameOver()
    {
        if(b_pass)
        {
            // If user has won
        }
        else
        {
            // If user has failed 
        }
        yield return null;
    }

    //in-game loop
    void Game()
    {
        // put updates here for when in in-game state
        if (!curHopTarget)
            SpawnHopTarget();

        
    }

    void SpawnHopTarget()
    {
        curHopTarget = CacheManager.ActivateRandom("Basic_HopTarget").GetComponent<HopTarget>();
        curHopTarget.transform.position = new Vector2(Random.Range(Constants.MIN_SPAWNABLE_X, Constants.MAX_SPAWNABLE_X), Random.Range(Constants.MIN_SPAWNABLE_Y, Constants.MAX_SPAWNABLE_Y));
        int colorIndex = Random.Range(0, Constants.NUMBER_OF_HOPCOLORS);
        curHopTarget.CurHopColor = (HopColor)colorIndex;
        curHopTarget.sprite.color = SetColorAppearance(curHopTarget.CurHopColor);

    }
    // call to start coroutine Hop
    public void StartHop(int colorIndex)
    {
        hopperColor = (HopColor)colorIndex;
        hopperSprite.color = SetColorAppearance(hopperColor);
        hopCoroutine = StartCoroutine(Hop());   // start hopping
    }
    public Color SetColorAppearance(HopColor hopColor)
    {
        switch (hopColor)
        {
            case HopColor.RED:
                return redAppearance;
            case HopColor.BLUE:
                return blueAppearance;
            case HopColor.YELLOW:
                return yellowAppearance;
            default:
                return Color.white;
        }
    }
    // animate hopping
    IEnumerator Hop()
    {
        float targetYPos = curHopTarget.transform.position.y;
        float t_percent = 0;
        float distanceToHop = targetYPos - player.transform.position.y;     //initial distance to be travelled by hopper to reach current hop target
        //while player not reached same y pos with target, move closer
        while(targetYPos > player.transform.position.y)
        {
            player.transform.Translate(Vector2.up * (targetYPos - player.transform.position.y) * 5 * Time.deltaTime);
            //animate scaling
            t_percent = (player.transform.position.y - restPos.y) / distanceToHop;      //percent of distance travelled in decimal
            player.transform.localScale = Vector2.one * hopperScaleAnimationCurve.Evaluate(t_percent);

            yield return null;
        }

        // this is the moment when player reached the target
        //put collision conditions here


        Debug.Log("Done hopping!");
    }
    void SETPATH()
    {
#if UNITY_EDITOR
        Path = Application.dataPath;
#else
		Path = Application.persistentDataPath;
#endif
    }
    
    public void ResetGame()
    {
        ChangeGameState(GAMESTATES.INIT);
    }

}
