using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private string playerTag = "Player";
    private bool triggered = false;

    [Header("Boss")]
    [SerializeField] private BossController boss;
    [SerializeField] private BossHealthUI bossHpUI;

    [Header("Mensaje inicial")]
    [TextArea(2, 4)]
    [SerializeField] private string startMessage = "⚠ ANCLA: Intruso detectado. Iniciando protocolo de contención.";
    [SerializeField] private float messageDuration = 2.5f;
    [Header("Cinemática")]
    [SerializeField] private float preAttackHold = 0.8f;

    [Tooltip("Duración del giro suave del player hacia el boss.")]
    [SerializeField] private float lookDuration = 0.6f;

    [SerializeField] private Behaviour[] disableDuringCinematic;

    [SerializeField] private bool freezePlayerRigidbody = true;

    [Header("Título del Jefe")]
    [SerializeField] private string bossTitle = "ENSAMBLADOR";
    [SerializeField] private float bossTitleHold = 2.2f;

    [Header("Música")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip bossMusic;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.85f;
    [SerializeField] private float musicFadeIn = 0.5f;
    [SerializeField] private bool loopBossMusic = true;
    [SerializeField] private PusherBotSpawner pusherSpawner;

    private Coroutine musicFadeCo;
    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;
        triggered = true;
        StartBossMusic();
        bossHpUI.gameObject.SetActive(true);
        bossHpUI.Show(true);

        if (BossTitleUI.Instance != null)
            BossTitleUI.Instance.ShowTitle(bossTitle, bossTitleHold);

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowText(startMessage);
            StartCoroutine(HideMsg());
        }

        if (pusherSpawner != null)
            pusherSpawner.BeginContinuous();

        StartCoroutine(CinematicSequence(other));
    }

    private IEnumerator CinematicSequence(Collider playerCol)
    {
        foreach (var b in disableDuringCinematic)
            if (b) b.enabled = false;

        var playerT = playerCol.transform.root != null ? playerCol.transform.root : playerCol.transform;
        var prb = playerT.GetComponent<Rigidbody>();
        bool hadRB = prb != null;
        bool prevKinematic = false;
        RigidbodyConstraints prevConstraints = RigidbodyConstraints.None;
        Vector3 prevVel = Vector3.zero, prevAng = Vector3.zero;

        if (freezePlayerRigidbody && hadRB)
        {
            prevKinematic = prb.isKinematic;
            prevConstraints = prb.constraints;
            prevVel = prb.velocity;
            prevAng = prb.angularVelocity;

            prb.velocity = Vector3.zero;
            prb.angularVelocity = Vector3.zero;
            prb.isKinematic = true;
        }

        if (boss != null)
        {
            Vector3 dir = boss.transform.position - playerT.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion startRot = playerT.rotation;
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);

                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / Mathf.Max(0.01f, lookDuration);
                    playerT.rotation = Quaternion.Slerp(startRot, targetRot, t);
                    yield return null;
                }
            }

            boss.ShowIdleVisual(true);
        }

        yield return new WaitForSeconds(messageDuration + preAttackHold);

        if (boss != null)
            boss.Activate();

        foreach (var b in disableDuringCinematic)
            if (b) b.enabled = true;

        if (freezePlayerRigidbody && hadRB)
        {
            prb.isKinematic = prevKinematic;
            prb.constraints = prevConstraints;
            prb.velocity = prevVel;
            prb.angularVelocity = prevAng;
        }
    }
    private IEnumerator HideMsg()
    {
        yield return new WaitForSeconds(messageDuration);
        if (DialogueUI.Instance != null) DialogueUI.Instance.HideText();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.2f);
        var col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Matrix4x4 old = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.matrix = old;
        }
    }

    private void EnsureMusicSource()
    {
        if (musicSource) return;

        var go = new GameObject("BossMusicSource");
        go.transform.SetParent(transform);
        musicSource = go.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = loopBossMusic;
        musicSource.spatialBlend = 0f;
        musicSource.priority = 128;
        musicSource.volume = 0f;
    }

    private IEnumerator FadeAudio(AudioSource src, float targetVol, float time)
    {
        time = Mathf.Max(0.0001f, time);
        float start = src.volume, t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            float k = t * t * (3f - 2f * t);
            src.volume = Mathf.Lerp(start, targetVol, k);
            yield return null;
        }
        src.volume = targetVol;
    }

    private void StartBossMusic()
    {
        if (!bossMusic)
        {
            Debug.LogWarning("Asigna el AudioClip de bossMusic en el Inspector.");
            return;
        }

        EnsureMusicSource();

        if (musicSource.isPlaying && musicSource.clip == bossMusic) return;

        musicSource.clip = bossMusic;
        musicSource.loop = loopBossMusic;
        musicSource.volume = 0f;
        musicSource.Play();

        if (musicFadeCo != null) StopCoroutine(musicFadeCo);
        musicFadeCo = StartCoroutine(FadeAudio(musicSource, musicVolume, musicFadeIn));
    }

}
