using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomTrigger : MonoBehaviour
{
    [Header("Música de esta zona")]
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 0.9f;
    public bool loop = true;
    public float fadeSeconds = 1.0f;

    [Header("Detección")]
    public string playerTag = "Player";

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (MusicManager.Instance == null) return;

        MusicManager.Instance.Play(clip, volume, fadeSeconds, loop);
    }
}
