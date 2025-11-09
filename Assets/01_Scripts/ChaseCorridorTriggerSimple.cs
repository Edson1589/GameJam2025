using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class ChaseCorridorTriggerSimple : MonoBehaviour
{
    [Header("Mensaje de alerta localizado")]
    [SerializeField] private LocalizedString localizedAlertText;
    [SerializeField] private float alertDuration = 2.5f;

    [Header("Spawn del dron")]
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private Transform droneSpawnPoint;

    [Header("Detección del jugador")]
    [SerializeField] private string playerTag = "Player";

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;

        triggered = true;

        if (DialogueUI.Instance != null && localizedAlertText != null)
        {
            DialogueUI.Instance.ShowText(localizedAlertText.GetLocalizedString());
            StartCoroutine(HideAlertAfter(alertDuration));
        }

        if (dronePrefab != null && droneSpawnPoint != null)
        {
            GameObject newDrone = Instantiate(
                dronePrefab,
                droneSpawnPoint.position,
                droneSpawnPoint.rotation
            );

            DroneChaserSimple chaser = newDrone.GetComponent<DroneChaserSimple>();
            if (chaser != null)
            {
                chaser.targetPlayer = other.transform;
                chaser.chaseActive = true;
            }
        }

        Debug.Log("ChaseCorridorTrigger: Dron desplegado y persecución iniciada.");
    }

    private IEnumerator HideAlertAfter(float t)
    {
        yield return new WaitForSeconds(t);
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.HideText();
        }
    }
}