using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPCNote : MonoBehaviour
{
    [Header("Mensaje que mostrará este NPC")]
    [TextArea(2, 5)]
    [SerializeField] private string npcMessage = "Zona inestable. Evita los pistones activos.";

    [Header("Detección del jugador")]
    [SerializeField] private string playerTag = "Player";

    [Header("UI flotante")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TMP_Text promptText;

    private bool playerInRange = false;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Presioné E");
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowText(npcMessage);
                Debug.Log("Mostrando mensaje: " + npcMessage);
            }
            else
            {
                Debug.LogWarning("DialogueUI.Instance es NULL, no hay HUD para mostrar texto.");
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

            if (promptText != null)
            {
                promptText.text = "[E] Conectar";
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
}
