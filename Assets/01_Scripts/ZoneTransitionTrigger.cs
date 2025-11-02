using System.Collections;
using UnityEngine;

public class ZoneTransitionTrigger : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private string playerTag = "Player";
    private bool triggered = false;

    [Header("Transición")]
    [SerializeField] private float fadeOut = 0.6f;
    [SerializeField] private float blackHold = 0.15f;
    [SerializeField] private float fadeIn = 0.6f;

    [Header("Zonas")]
    [SerializeField] private GameObject currentZoneRoot;
    [SerializeField] private GameObject nextZoneRoot;
    [SerializeField] private Transform nextZoneSpawnPoint;

    [Header("Opcional")]
    [SerializeField] private bool alignPlayerRotationToSpawn = true;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;
        triggered = true;

        StartCoroutine(DoTransition(other.transform));
    }

    private IEnumerator DoTransition(Transform player)
    {
        yield return ScreenFader.Instance.FadeOutIn(fadeOut, blackHold, fadeIn, () =>
        {
            if (nextZoneSpawnPoint)
            {
                var rb = player.GetComponent<Rigidbody>();
                if (rb) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
                player.position = nextZoneSpawnPoint.position;
                if (alignPlayerRotationToSpawn)
                    player.rotation = nextZoneSpawnPoint.rotation;
            }

            if (currentZoneRoot) currentZoneRoot.SetActive(false);
            if (nextZoneRoot) nextZoneRoot.SetActive(true);
        });

        gameObject.SetActive(false);
    }
}
