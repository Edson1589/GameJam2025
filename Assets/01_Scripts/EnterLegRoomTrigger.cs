using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterLegRoomTrigger : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private string playerTag = "Player";
    private bool triggered = false;

    [Header("Cápsula")]
    [SerializeField] private LegCapsuleController capsuleController;

    [Header("Mensaje inicial")]
    [TextArea(2, 4)]
    [SerializeField] private string message = "Recolector biomecánico localizado.\nMódulo Piernas disponible.";
    [SerializeField] private float messageDuration = 2.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;
        triggered = true;

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowText(message);
            StartCoroutine(HideMessageAfterDelay());
        }

        if (capsuleController != null)
        {
            capsuleController.BeginSequence();
        }
        else
        {
            Debug.LogWarning("No se asignó capsuleController en EnterLegRoomTrigger.");
        }
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.HideText();
        }
    }
}
