using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD : MonoBehaviour
{
    public Text scoreUI;

    public void UpdateScoreUI(string text)
    {
        scoreUI.text = text;
    }
}
