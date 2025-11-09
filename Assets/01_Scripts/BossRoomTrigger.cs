using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class BossRoomTrigger : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private string playerTag = "Player";
    private bool triggered = false;

    [Header("Boss")]
    [SerializeField] private BossController boss;
    [SerializeField] private BossHealthUI bossHpUI;

    [Header("Mensaje inicial localizado")]
    [SerializeField] private LocalizedString localizedStartMessage;
    [SerializeField] private float messageDuration = 2.5f;

    [Header("Cinemática")]
    [SerializeField] private float preAttackHold = 0.8f;

    [Tooltip("Duración del giro suave del player hacia el boss.")]
    [SerializeField] private float lookDuration = 0.6f;

    [SerializeField] private Behaviour[] disableDuringCinematic;

    [SerializeField] private bool freezePlayerRigidbody = true;

    [Header("Título del Jefe localizado")]
    [SerializeField] private LocalizedString localizedBossTitle;
    [SerializeField] private float bossTitleHold = 2.2f;

    [SerializeField] private PusherBotSpawner pusherSpawner;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;
        triggered = true;

        bossHpUI.gameObject.SetActive(true);
        bossHpUI.Show(true);

        if (BossTitleUI.Instance != null && localizedBossTitle != null)
            BossTitleUI.Instance.ShowTitle(localizedBossTitle.GetLocalizedString(), bossTitleHold);

        if (DialogueUI.Instance != null && localizedStartMessage != null)
        {
            DialogueUI.Instance.ShowText(localizedStartMessage.GetLocalizedString());
            StartCoroutine(HideMsg());
        }

        if (pusherSpawner != null)
            pusherSpawner.BeginContinuous();

        StartCoroutine(CinematicSequence(other));
    }

    private IEnumerator CinematicSequence(Collider playerCol)
    {
        foreach (var b in disableDuringCinematic)
            if (b) b.enabled = false;

        var playerT = playerCol.transform.root != null ? playerCol.transform.root : playerCol.transform;
        var prb = playerT.GetComponent<Rigidbody>();
        bool hadRB = prb != null;
        bool prevKinematic = false;
        RigidbodyConstraints prevConstraints = RigidbodyConstraints.None;
        Vector3 prevVel = Vector3.zero, prevAng = Vector3.zero;

        if (freezePlayerRigidbody && hadRB)
        {
            prevKinematic = prb.isKinematic;
            prevConstraints = prb.constraints;
            prevVel = prb.velocity;
            prevAng = prb.angularVelocity;

            prb.velocity = Vector3.zero;
            prb.angularVelocity = Vector3.zero;
            prb.isKinematic = true;
        }

        if (boss != null)
        {
            Vector3 dir = boss.transform.position - playerT.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion startRot = playerT.rotation;
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);

                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / Mathf.Max(0.01f, lookDuration);
                    playerT.rotation = Quaternion.Slerp(startRot, targetRot, t);
                    yield return null;
                }
            }

            boss.ShowIdleVisual(true);
        }

        yield return new WaitForSeconds(messageDuration + preAttackHold);

        if (boss != null)
            boss.Activate();

        foreach (var b in disableDuringCinematic)
            if (b) b.enabled = true;

        if (freezePlayerRigidbody && hadRB)
        {
            prb.isKinematic = prevKinematic;
            prb.constraints = prevConstraints;
            prb.velocity = prevVel;
            prb.angularVelocity = prevAng;
        }
    }

    private IEnumerator HideMsg()
    {
        yield return new WaitForSeconds(messageDuration);
        if (DialogueUI.Instance != null) DialogueUI.Instance.HideText();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.2f);
        var col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Matrix4x4 old = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.matrix = old;
        }
    }
}