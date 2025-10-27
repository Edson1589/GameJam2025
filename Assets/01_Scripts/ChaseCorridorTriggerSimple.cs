using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseCorridorTriggerSimple : MonoBehaviour
{
    [Header("Mensaje de alerta")]
    [TextArea(2, 4)]
    [SerializeField] private string alertText = "ALERTA: Unidad R.U.B.O. detectada.\nProcediendo a captura.";
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

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowText(alertText);
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
