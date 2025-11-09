using UnityEngine;

public class KillVolume : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        var health = other.GetComponent<PlayerHealth>();
        if (health) health.KillInstant();
        else other.GetComponent<PlayerRespawnHandler>()?.RespawnNow();
    }
}
