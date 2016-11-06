using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD : MonoBehaviour
{
    public Text scoreUI;
    public CanvasGroup whiteScreen;

    private Coroutine whiteToFadeCoroutine;

    public void UpdateScoreUI(string text)
    {
        scoreUI.text = text;
    }

    public void WhiteToFade(float duration)
    {
        if (whiteToFadeCoroutine != null)
            StopCoroutine(whiteToFadeCoroutine);
        whiteToFadeCoroutine = StartCoroutine(AnimateWhiteToFade(duration));
    }
    private IEnumerator AnimateWhiteToFade(float duration)
    {
        whiteScreen.alpha = 1;
        while (whiteScreen.alpha > 0)
        {
            whiteScreen.alpha -= Time.deltaTime / duration;
            yield return null;
        }
    }
}
