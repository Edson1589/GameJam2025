using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;

public class DoorWithKey : MonoBehaviour
{
    [Header("Puerta física que se mueve")]
    [SerializeField] private Transform doorMesh;

    [Header("Apertura")]
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 3f, 0f);
    [SerializeField] private float openSpeed = 3f;

    [Header("UI flotante cerca de la puerta")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TMP_Text promptText;

    [Header("Textos localizados")]
    [SerializeField] private LocalizedString localizedOpenText; // "[E] Abrir" / "[E] Open"
    [SerializeField] private LocalizedString localizedNeedKeyText; // "Encuentra la llave" / "Find the key"

    [Header("Detección del jugador")]
    [SerializeField] private string playerTag = "Player";

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool isOpen = false;
    private bool playerInRange = false;
    private PlayerInventory playerInv;

    private void Start()
    {
        if (doorMesh == null)
            doorMesh = this.transform;

        closedPos = doorMesh.position;
        openPos = closedPos + openOffset;

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (playerInv == null) return;

        if (!isOpen)
        {
            if (playerInv.HasKey)
            {
                if (promptUI != null && promptText != null && localizedOpenText != null)
                {
                    promptUI.SetActive(true);
                    promptText.text = localizedOpenText.GetLocalizedString();
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartCoroutine(OpenDoor());
                }
            }
            else
            {
                if (promptUI != null && promptText != null && localizedNeedKeyText != null)
                {
                    promptUI.SetActive(true);
                    promptText.text = localizedNeedKeyText.GetLocalizedString();
                }
            }
        }
        else
        {
            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }

    private IEnumerator OpenDoor()
    {
        isOpen = true;

        while (Vector3.Distance(doorMesh.position, openPos) > 0.01f)
        {
            doorMesh.position = Vector3.MoveTowards(
                doorMesh.position,
                openPos,
                openSpeed * Time.deltaTime
            );
            yield return null;
        }

        doorMesh.position = openPos;

        if (promptUI != null)
            promptUI.SetActive(false);

        Debug.Log(">> DoorWithKey: Puerta abierta.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            playerInv = other.GetComponent<PlayerInventory>();

            if (promptUI != null)
                promptUI.SetActive(true);
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
        }
    }
}