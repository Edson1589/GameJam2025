using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DroneChaserSimple : MonoBehaviour
{
    [Header("Objetivo a perseguir")]
    public Transform targetPlayer;

    [Header("Refs")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform droneVisual;
    [SerializeField] private float hoverHeightOffset = 1.0f;

    [Header("Hover bob (solo visual)")]
    [SerializeField] private float bobAmp = 0.1f;
    [SerializeField] private float bobFreq = 4f;

    [Header("Estado")]
    public bool chaseActive = false;

    [Header("Audio Loop")]
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private AudioClip loopClip;
    [SerializeField, Range(0f, 1f)] private float loopVolume = 0.6f;
    [SerializeField] private float loopFadeIn = 0.25f;
    [SerializeField] private float loopFadeOut = 0.2f;
    [SerializeField] private bool autoStartLoopOnEnable = true;

    private bool lastChaseActive;
    private Coroutine fadeCo;

    private void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.updateRotation = true;
            agent.updateUpAxis = true;
        }

        SetupLoopAudio();

        lastChaseActive = !chaseActive;
        EvaluateLoopState();
    }

    private void Update()
    {
        if (!chaseActive) return;
        if (targetPlayer == null) return;
        if (agent == null) return;

        agent.SetDestination(targetPlayer.position);

        if (droneVisual != null)
        {
            Vector3 basePos = transform.position;
            basePos.y += hoverHeightOffset;
            basePos.y += Mathf.Sin(Time.time * bobFreq) * bobAmp;

            droneVisual.position = basePos;

            Vector3 lookDir = targetPlayer.position - droneVisual.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion rot = Quaternion.LookRotation(lookDir, Vector3.up);
                droneVisual.rotation = Quaternion.Slerp(droneVisual.rotation, rot, 15f * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRespawnHandler resp = other.GetComponent<PlayerRespawnHandler>();
            if (resp != null)
            {
                resp.RespawnNow();
            }

            Debug.Log("El dron atrapó al jugador -> respawn.");
        }
    }

    private void SetupLoopAudio()
    {
        if (!loopSource)
        {
            Transform host = droneVisual ? droneVisual : transform;
            loopSource = host.gameObject.AddComponent<AudioSource>();
        }

        loopSource.clip = loopClip;
        loopSource.loop = true;
        loopSource.playOnAwake = false;
        loopSource.volume = 0f;
        loopSource.spatialBlend = 1f;
        loopSource.dopplerLevel = 0f;
        loopSource.rolloffMode = AudioRolloffMode.Linear;
        loopSource.minDistance = 4f;
        loopSource.maxDistance = 30f;

        if (autoStartLoopOnEnable && chaseActive && loopClip)
        {
            loopSource.Play();
            StartFade(loopSource, loopVolume, loopFadeIn);
        }
    }

    private void EvaluateLoopState()
    {
        if (lastChaseActive == chaseActive) return;
        lastChaseActive = chaseActive;

        if (!loopSource || !loopClip) return;

        if (chaseActive)
        {
            if (!loopSource.isPlaying) loopSource.Play();
            StartFade(loopSource, loopVolume, loopFadeIn);
        }
        else
        {
            StartFadeAndStop(loopSource, loopFadeOut);
        }
    }

    private void StartFade(AudioSource src, float to, float time)
    {
        if (!src) return;
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeCo(src, src.volume, to, time, false));
    }

    private void StartFadeAndStop(AudioSource src, float time)
    {
        if (!src) return;
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeCo(src, src.volume, 0f, time, true));
    }

    private IEnumerator FadeCo(AudioSource src, float from, float to, float t, bool stopAtEnd)
    {
        if (t <= 0f)
        {
            src.volume = to;
            if (stopAtEnd && to <= 0f) src.Stop();
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < t)
        {
            elapsed += Time.deltaTime;
            src.volume = Mathf.Lerp(from, to, elapsed / t);
            yield return null;
        }
        src.volume = to;
        if (stopAtEnd && to <= 0f) src.Stop();
        fadeCo = null;
    }

    private void OnDisable()
    {
        if (loopSource && loopSource.isPlaying)
        {
            loopSource.Stop();
            loopSource.volume = 0f;
        }
    }
}
