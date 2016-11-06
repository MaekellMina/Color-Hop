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

    public const float OFFSCREEN_POINT_Y = 6f;

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

    public enum HOPPERSTATES
    {
        IDLE,           // not moving
        STRIFING,       // moving side to side
        HOPPING         // in the process of hopping
    }
    public HOPPERSTATES hopperState = HOPPERSTATES.IDLE;

    public enum STRIFE_DIR
    {
        LEFT, RIGHT
    }
    public STRIFE_DIR playerStrifeDir = STRIFE_DIR.LEFT;

    private bool b_gameover;                // Is the game over?

    bool callOnce = true;                   // Used when changing the game state bool for calling function/code once in the game
    bool callOnce2 = true;                  // Used when changin the hopper state bool for calling function/code once in the game

    //--------public game fields
    public GameObject player;               // reference to player token AKA Hopper
    public float playerRadius = .5f;        // radius of player (for collision detection)
    public float strifeSpeed;               // hopper moves side to side speed
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
        ChangeGameState(GAMESTATES.GAMEOVER);
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
                    

                    //
                    callOnce = false;
                }
                // -- Put codes that are needed to be called only once -- //
                //Do the setup for the game here.
                b_gameover = false;
                gameScore = 0;
                UpdateScore(gameScore);
                player.transform.position = restPos;
                hopperState = HOPPERSTATES.IDLE;

                ChangeGameState(GAMESTATES.INGAME);

                //change gamestate after running init once
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
                    Debug.Log("GAME OVER");
                    StartCoroutine(GameOver());
                    //
                    callOnce = false;
                }
                break;

        }
    }
    public void ChangeGameState(int state)  //for button click event (just in case)
    {
        callOnce = true;
        gameState = (GAMESTATES)state;
    }
    public void ChangeGameState(GAMESTATES state)
    {

        callOnce = true;        // Set to true so every time the state change, there's a place to call some code once in the loop
        gameState = state;
    }
    public void ChangeHopperState(HOPPERSTATES state)
    {
        callOnce2 = true;
        hopperState = state;
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
        Debug.Log("RESTARTING GAME");
        gameHUD.WhiteToFade(1);
        ChangeGameState(GAMESTATES.INIT);
        yield return null;
    }

    //in-game loop
    void Game()
    {
        // put updates here for when in in-game state
        if (!curHopTarget)
            SpawnHopTarget();
        

        switch(hopperState)
        {
            case HOPPERSTATES.IDLE:
                if (callOnce2)
                {
                    // -- Put codes that are needed to be called only once -- //

                    //
                    callOnce2 = false;
                }
                break;
            case HOPPERSTATES.STRIFING:
                if (callOnce2)
                {
                    // -- Put codes that are needed to be called only once -- //
                    
                    //
                    callOnce2 = false;
                }
                switch (playerStrifeDir)
                {
                    case STRIFE_DIR.LEFT:
                        player.transform.Translate(Vector2.left * strifeSpeed * Time.deltaTime);
                        if (player.transform.position.x <= Constants.MIN_SPAWNABLE_X)
                            playerStrifeDir = STRIFE_DIR.RIGHT;
                        break;
                    case STRIFE_DIR.RIGHT:
                        player.transform.Translate(Vector2.right * strifeSpeed * Time.deltaTime);
                        if (player.transform.position.x >= Constants.MAX_SPAWNABLE_X)
                            playerStrifeDir = STRIFE_DIR.LEFT;
                        break;
                }
                break;
            case HOPPERSTATES.HOPPING:
                if (callOnce2)
                {
                    // -- Put codes that are needed to be called only once -- //

                    //
                    callOnce2 = false;
                }
                break;
        }
        
    }

    void SpawnHopTarget()
    {
        curHopTarget = CacheManager.ActivateRandom("Basic_HopTarget").GetComponent<HopTarget>();
        Vector2 hopTargetPos;
        
        if (hopperState == HOPPERSTATES.IDLE)       // if hopper is not moving/idle, spawn same X with player
            hopTargetPos = new Vector2(player.transform.position.x, Random.Range(Constants.MIN_SPAWNABLE_Y, Constants.MAX_SPAWNABLE_Y));
        else
            hopTargetPos = new Vector2(Random.Range(Constants.MIN_SPAWNABLE_X, Constants.MAX_SPAWNABLE_X), Random.Range(Constants.MIN_SPAWNABLE_Y, Constants.MAX_SPAWNABLE_Y));

        //spawn initially off screen then animate to supposed position
        curHopTarget.transform.position = new Vector2(hopTargetPos.x, Constants.OFFSCREEN_POINT_Y);
        StartCoroutine(PositionHopTarget(hopTargetPos));

        int colorIndex = Random.Range(0, Constants.NUMBER_OF_HOPCOLORS);
        curHopTarget.CurHopColor = (HopColor)colorIndex;
        curHopTarget.sprite.color = SetColorAppearance(curHopTarget.CurHopColor);

    }
    IEnumerator PositionHopTarget(Vector2 pos)
    {
        while (System.Math.Round(curHopTarget.transform.position.y, 1) > System.Math.Round(pos.y,1))
        {
            curHopTarget.transform.position = Vector2.Lerp(curHopTarget.transform.position, pos, 3 * Time.deltaTime);
            yield return null;
        }
    }
    // call to start coroutine Hop
    public void StartHop(int colorIndex)
    {
        //only hop when not already hopping
        if (hopperState != HOPPERSTATES.HOPPING)
        {
            hopperColor = (HopColor)colorIndex;
            hopperSprite.color = SetColorAppearance(hopperColor);
            ChangeHopperState(HOPPERSTATES.HOPPING);
            hopCoroutine = StartCoroutine(Hop());   // start hopping
        }
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
        while(System.Math.Round(targetYPos,1) > System.Math.Round(player.transform.position.y, 1))
        {
            player.transform.Translate(Vector2.up * (targetYPos - player.transform.position.y) * 5 * Time.deltaTime);
            //animate scaling
            t_percent = (player.transform.position.y - restPos.y) / distanceToHop;      //percent of distance travelled in decimal
            player.transform.localScale = Vector2.one * hopperScaleAnimationCurve.Evaluate(t_percent);
            Debug.Log(targetYPos + " " + player.transform.position.y);
            yield return null;
        }

        // this is the moment when player reached the target
        //put collision conditions here

        Debug.Log("CHECKING COLLISION");
        if (curHopTarget.GetComponent<Collider2D>() == Physics2D.OverlapCircle(player.transform.position, playerRadius) &&
            hopperColor == curHopTarget.CurHopColor)
        {
            //successful hit
            curHopTarget.Explode();
            curHopTarget = null;
            StartCoroutine(ReturnToRestPos());
            gameScore++;
            UpdateScore(gameScore);
        }
        else
        {
            //missed
            curHopTarget.Explode();
            curHopTarget = null;
            EventsManager.OnGameOver.Invoke();
        }


        Debug.Log("Done hopping!");
    }
    IEnumerator ReturnToRestPos()
    {
        while (System.Math.Round(player.transform.position.y, 1) > System.Math.Round(restPos.y, 1))
        {
            player.transform.position = Vector2.Lerp(player.transform.position, restPos, 3 * Time.deltaTime);
            yield return null;
        }
        Debug.Log("RETURNED TO REST POS");
        ChangeHopperState(HOPPERSTATES.STRIFING);
    }

    void UpdateScore(int score)
    {
        gameScore = score;
        gameHUD.UpdateScoreUI(score.ToString());

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
