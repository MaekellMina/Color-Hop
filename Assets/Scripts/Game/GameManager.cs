using UnityEngine;
using System.Collections;
using System;

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


    //--------private game fields


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
