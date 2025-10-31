using System.Collections;
using UnityEngine;
using TMPro;

public class BossTitleUI : MonoBehaviour
{
    public static BossTitleUI Instance;

    [Header("Refs")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text title;

    [Header("Fades")]
    [SerializeField] private float fadeInTime = 0.3f;
    [SerializeField] private float fadeOutTime = 0.35f;

    private Coroutine running;

    void Awake()
    {
        Instance = this;
        if (group) group.alpha = 0f;
        gameObject.SetActive(true);
    }

    public void ShowTitle(string text, float holdSeconds)
    {
        if (!title || !group) return;

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(ShowRoutine(text, holdSeconds));
    }

    private IEnumerator ShowRoutine(string text, float hold)
    {
        title.text = text;


        yield return FadeTo(1f, fadeInTime);
        yield return new WaitForSeconds(hold);

        yield return FadeTo(0f, fadeOutTime);

        running = null;
    }

    private IEnumerator FadeTo(float target, float time)
    {
        float start = group.alpha;
        float t = 0f;
        time = Mathf.Max(0.0001f, time);

        while (t < 1f)
        {
            t += Time.deltaTime / time;
            float k = t * t * (3f - 2f * t);
            group.alpha = Mathf.Lerp(start, target, k);
            yield return null;
        }
        group.alpha = target;
    }
}
