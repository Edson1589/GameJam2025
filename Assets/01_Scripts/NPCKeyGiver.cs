using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;

public class NPCKeyGiver : MonoBehaviour
{
    [Header("Mensajes Localizados del NPC")]
    [SerializeField] private LocalizedString localizedFirstMessage;
    [SerializeField] private LocalizedString localizedRepeatMessage;

    [Header("Detección del jugador")]
    [SerializeField] private string playerTag = "Player";

    [Header("UI flotante")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private LocalizedString localizedPromptText;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip sfxGive;
    [SerializeField] private AudioClip sfxRepeat;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.98f, 1.02f);

    private bool playerInRange = false;
    private bool keyGiven = false;
    private PlayerInventory playerInv;

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
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (playerInv != null)
            {
                if (!keyGiven)
                {
                    playerInv.GiveKey();
                    keyGiven = true;
                    PlayOne(sfxGive);

                    if (DialogueUI.Instance != null && localizedFirstMessage != null)
                    {
                        DialogueUI.Instance.ShowText(localizedFirstMessage.GetLocalizedString());
                    }
                    Debug.Log(">> NPCKeyGiver: llave entregada al jugador.");
                }
                else
                {
                    PlayOne(sfxRepeat);
                    if (DialogueUI.Instance != null && localizedRepeatMessage != null)
                    {
                        DialogueUI.Instance.ShowText(localizedRepeatMessage.GetLocalizedString());
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (DialogueUI.Instance != null)
                DialogueUI.Instance.HideText();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            playerInv = other.GetComponent<PlayerInventory>();

            if (promptUI != null)
                promptUI.SetActive(true);

            if (promptText != null && localizedPromptText != null)
                promptText.text = localizedPromptText.GetLocalizedString();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            playerInv = null;

            if (promptUI != null)
                promptUI.SetActive(false);

            if (DialogueUI.Instance != null)
                DialogueUI.Instance.HideText();
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
            // Fallback: si no hay AudioSource por alguna razón
            AudioSource.PlayClipAtPoint(clip, transform.position, sfxVolume * volMul);
        }
    }
}