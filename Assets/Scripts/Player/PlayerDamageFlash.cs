using System;
using System.Collections;
using UnityEngine;

public class PlayerDamageFlash : MonoBehaviour
{
    [SerializeField] CanvasGroup flashGroup;
    [SerializeField, Range(0f,1f)] float maxAlpha = .5f;
    [SerializeField] float fadeDuration = .4f;

    Coroutine routine;

    void Awake()
    {
        if(flashGroup != null)
        {
            flashGroup.alpha = 0f;
        }
    }

    public void Flash(float intensity = 1f)
    {
        if(routine != null)
        {
            StopCoroutine(routine);
        }
        
        routine = StartCoroutine(FlashRoutine(Mathf.Clamp01(intensity)));
    }

    IEnumerator FlashRoutine(float intensity)
    {
        float startAlpha = maxAlpha * intensity;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            flashGroup.alpha = Mathf.Lerp(startAlpha, 0f, t/fadeDuration);
            yield return null;
        }

        flashGroup.alpha = 0f;
        routine = null;
    }
}
