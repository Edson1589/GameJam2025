using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;

public class NPCNote : MonoBehaviour
{
    [Header("Mensaje Localizado")]
    [SerializeField] private LocalizedString localizedNpcMessage;

    [Header("Detección del jugador")]
    [SerializeField] private string playerTag = "Player";

    [Header("UI flotante")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private LocalizedString localizedPromptText;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip sfxOpen;
    [SerializeField] private AudioClip sfxClose;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.98f, 1.02f);

    private bool playerInRange = false;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        if (!sfxSource)
        {
            sfxSource = GetComponent<AudioSource>();
            if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 1f;
            sfxSource.dopplerLevel = 0f;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Presioné E");
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (DialogueUI.Instance != null && localizedNpcMessage != null)
            {
                string message = localizedNpcMessage.GetLocalizedString();
                DialogueUI.Instance.ShowText(message);
                PlayOne(sfxOpen);
                Debug.Log("Mostrando mensaje: " + message);
            }
            else
            {
                Debug.LogWarning("DialogueUI.Instance es NULL o localizedNpcMessage no está configurado.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.HideText();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;

            if (promptUI != null)
            {
                promptUI.SetActive(true);
            }

            if (promptText != null && localizedPromptText != null)
            {
                promptText.text = localizedPromptText.GetLocalizedString();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;

            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }

            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.HideText();
            }
        }
    }

    private void PlayOne(AudioClip clip, float volMul = 1f)
    {
        if (!clip) return;
        if (sfxSource)
        {
            sfxSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
            sfxSource.PlayOneShot(clip, sfxVolume * volMul);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, sfxVolume * volMul);
        }
    }
}