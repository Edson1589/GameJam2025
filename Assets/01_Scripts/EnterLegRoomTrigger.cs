using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class EnterLegRoomTrigger : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private string playerTag = "Player";
    private bool triggered = false;

    [Header("Cápsula")]
    [SerializeField] private LegCapsuleController capsuleController;

    [Header("Mensaje inicial localizado")]
    [SerializeField] private LocalizedString localizedMessage;
    [SerializeField] private float messageDuration = 2.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;

        triggered = true;

        if (DialogueUI.Instance != null && localizedMessage != null)
        {
            DialogueUI.Instance.ShowText(localizedMessage.GetLocalizedString());
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