using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private bool startBlack = false;
    [SerializeField] private float defaultFadeDuration = 0.6f;

    private CanvasGroup group;
    private Coroutine running;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        group = GetComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        SetInstant(startBlack ? 1f : 0f);
    }

    public void SetInstant(float a)
    {
        if (running != null) StopCoroutine(running);
        group.alpha = Mathf.Clamp01(a);
    }

    public Coroutine FadeTo(float targetAlpha, float duration = -1f)
    {
        if (duration < 0f) duration = defaultFadeDuration;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(CoFadeTo(targetAlpha, duration));
        return running;
    }

    public Coroutine FadeOutIn(float outDur, float blackHold, float inDur, Action midAction)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(CoFadeOutIn(outDur, blackHold, inDur, midAction));
        return running;
    }

    private IEnumerator CoFadeTo(float target, float duration)
    {
        float start = group.alpha;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, duration);
            group.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        group.alpha = target;
        running = null;
    }

    private IEnumerator CoFadeOutIn(float outDur, float hold, float inDur, Action mid)
    {
        yield return CoFadeTo(1f, outDur);
        mid?.Invoke();
        if (hold > 0f) yield return new WaitForSecondsRealtime(hold);
        yield return CoFadeTo(0f, inDur);
        running = null;
    }
}
