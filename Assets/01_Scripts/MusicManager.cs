using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Opcional")]
    public AudioMixerGroup mixerGroup;
    public float defaultFadeSeconds = 1.0f;

    private AudioSource[] sources = new AudioSource[2];
    private int activeIndex = 0;
    private Coroutine xfadeCo;
    private AudioClip currentClip;
    [SerializeField] private bool stopOnSceneChange = true;

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnDisable()
    {
        if (Instance == this)
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (!stopOnSceneChange) return;
        StopMusic(defaultFadeSeconds);
    }
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sources[0] = gameObject.AddComponent<AudioSource>();
        sources[1] = gameObject.AddComponent<AudioSource>();
        foreach (var s in sources)
        {
            s.loop = true;
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            if (mixerGroup) s.outputAudioMixerGroup = mixerGroup;
        }
    }

    public void Play(AudioClip clip, float targetVolume = 1f, float fadeSeconds = -1f, bool loop = true, float pitch = 1f)
    {
        if (clip == null) return;
        if (Mathf.Approximately(fadeSeconds, -1f)) fadeSeconds = defaultFadeSeconds;

        if (currentClip == clip && sources[activeIndex].isPlaying)
        {
            sources[activeIndex].loop = loop;
            sources[activeIndex].pitch = pitch;
            StartSetVolume(targetVolume, fadeSeconds);
            return;
        }

        int next = 1 - activeIndex;
        var newSrc = sources[next];
        var oldSrc = sources[activeIndex];

        newSrc.clip = clip;
        newSrc.volume = 0f;
        newSrc.pitch = pitch;
        newSrc.loop = loop;
        newSrc.Play();

        if (xfadeCo != null) StopCoroutine(xfadeCo);
        xfadeCo = StartCoroutine(Crossfade(oldSrc, newSrc, targetVolume, fadeSeconds));

        currentClip = clip;
        activeIndex = next;
    }

    public void StopMusic(float fadeSeconds = -1f)
    {
        if (Mathf.Approximately(fadeSeconds, -1f)) fadeSeconds = defaultFadeSeconds;

        if (xfadeCo != null) StopCoroutine(xfadeCo);
        xfadeCo = StartCoroutine(FadeOutThenStop(sources[activeIndex], fadeSeconds));
        currentClip = null;
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to, float toVolume, float seconds)
    {
        if (seconds <= 0f)
        {
            if (from.isPlaying) from.Stop();
            to.volume = toVolume;
            yield break;
        }

        float t = 0f;
        float startFrom = from.volume;
        float startTo = to.volume;

        while (t < seconds)
        {
            t += Time.deltaTime;
            float k = t / seconds;
            if (from != null) from.volume = Mathf.Lerp(startFrom, 0f, k);
            if (to != null) to.volume = Mathf.Lerp(startTo, toVolume, k);
            yield return null;
        }

        if (from != null && from.isPlaying) from.Stop();
        if (to != null) to.volume = toVolume;
    }

    private IEnumerator FadeOutThenStop(AudioSource src, float seconds)
    {
        if (src == null) yield break;
        if (seconds <= 0f) { src.Stop(); yield break; }

        float t = 0f;
        float start = src.volume;

        while (t < seconds)
        {
            t += Time.deltaTime;
            float k = t / seconds;
            src.volume = Mathf.Lerp(start, 0f, k);
            yield return null;
        }

        src.Stop();
        src.volume = 0f;
    }

    private void StartSetVolume(float target, float seconds)
    {
        if (xfadeCo != null) StopCoroutine(xfadeCo);
        xfadeCo = StartCoroutine(FadeVolume(sources[activeIndex], target, seconds));
    }

    private IEnumerator FadeVolume(AudioSource src, float target, float seconds)
    {
        if (src == null) yield break;
        if (seconds <= 0f) { src.volume = target; yield break; }

        float t = 0f;
        float start = src.volume;

        while (t < seconds)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(start, target, t / seconds);
            yield return null;
        }

        src.volume = target;
    }
}
