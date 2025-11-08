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
    [SerializeField] private LocalizedString localizedPromptText; // "[E] Conectar"

    private bool playerInRange = false;
    private bool keyGiven = false;
    private PlayerInventory playerInv;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
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

                    if (DialogueUI.Instance != null && localizedFirstMessage != null)
                    {
                        DialogueUI.Instance.ShowText(localizedFirstMessage.GetLocalizedString());
                    }
                    Debug.Log(">> NPCKeyGiver: llave entregada al jugador.");
                }
                else
                {
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
}