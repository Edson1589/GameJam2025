using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPCKeyGiver : MonoBehaviour
{
    [Header("Mensajes del NPC")]
    [TextArea(2, 5)]
    [SerializeField] private string firstMessage = "Toma esto... te abrirá el camino. No dejes que A.N.C.L.A. te copie.";
    [TextArea(2, 5)]
    [SerializeField] private string repeatMessage = "Ya tienes la llave. Ve a la puerta principal.";

    [Header("Detección del jugador")]
    [SerializeField] private string playerTag = "Player";

    [Header("UI flotante")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TMP_Text promptText;

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

                    if (DialogueUI.Instance != null)
                    {
                        DialogueUI.Instance.ShowText(firstMessage);
                    }

                    Debug.Log(">> NPCKeyGiver: llave entregada al jugador.");
                }
                else
                {
                    if (DialogueUI.Instance != null)
                    {
                        DialogueUI.Instance.ShowText(repeatMessage);
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

            if (promptText != null)
                promptText.text = "[E] Conectar";
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
