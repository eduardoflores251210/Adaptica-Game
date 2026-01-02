using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeToBlck : MonoBehaviour //si se me olvido la a 
{
    public Image blackPanel;       // La sábana negra
    public float fadeDuration = 1.5f;

    public void StartFadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    public void StartFadeIn()
    {
        StartCoroutine(FadeInCoroutine());
    }

    IEnumerator FadeOutCoroutine()
    {
        Color c = new(0,0,0,0);
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            c.a = t / fadeDuration;
            blackPanel.color = c;
            Debug.Log(blackPanel.color.a.ToString());
            yield return null;
        }
        c.a = 1;
        blackPanel.color = c;
    }

    IEnumerator FadeInCoroutine()
    {
        Color c = Color.black;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            c.a = 1 - (t / fadeDuration);
            blackPanel.color = c;
            Debug.Log(blackPanel.color.a.ToString());
            yield return null;
        }
        c.a = 0;
        blackPanel.color = c;
    }
}
