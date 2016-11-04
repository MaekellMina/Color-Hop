using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class EventsManager : MonoBehaviour
{
    public static UnityEvent OnGameWin;
    public static UnityEvent OnGameOver;
    public static UnityEvent OnGameReset;

    public static UnityEvent OnGameResumed;
    public static UnityEvent OnGamePaused;

    void Awake()
    {
        OnGameWin = new UnityEvent();
        OnGameOver = new UnityEvent();
        OnGameReset = new UnityEvent();

        OnGameResumed = new UnityEvent();
        OnGamePaused = new UnityEvent();
    }
}