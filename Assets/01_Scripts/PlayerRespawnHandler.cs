using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawnHandler : MonoBehaviour
{
    [Header("Punto donde reaparece el jugador")]
    [SerializeField] private Transform respawnPoint;

    [Header("Opcional si usas Rigidbody")]
    [SerializeField] private Rigidbody rb;

    public void RespawnNow()
    {
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("Jugador respawneado al inicio del pasillo 4.");

        // Notificar que el jugador ha respawneado (para restaurar boss y munición)
        OnPlayerRespawned();
    }

    private void OnPlayerRespawned()
    {
        // Restaurar salud del boss
        AnchorMother boss = FindObjectOfType<AnchorMother>();
        if (boss != null)
        {
            boss.RestoreFullHealth();
        }

        // Restaurar todos los AmmoPickup
        AmmoPickup[] allAmmoPickups = FindObjectsOfType<AmmoPickup>();
        foreach (AmmoPickup pickup in allAmmoPickups)
        {
            pickup.Respawn();
        }

        Debug.Log($"PlayerRespawnHandler: Boss restaurado y {allAmmoPickups.Length} municiones reaparecieron.");
    }
}
