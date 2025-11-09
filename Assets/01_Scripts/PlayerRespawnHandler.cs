using System.Collections;
using UnityEngine;

public class PlayerRespawnHandler : MonoBehaviour
{
    [Header("Fallback (opcional) si no hay manager")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private bool alignRotationToSpawn = true;

    [Header("Física (opcional)")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CharacterController characterController;

    [SerializeField] private int maxHP = 100;
    private float progress = 0f;
    public void RespawnNow()
    {
        TeleportToSpawn();
        OnPlayerRespawned();
    }

    private void TeleportToSpawn()
    {
        Transform spawn = ZoneSpawnManager.Instance ? ZoneSpawnManager.Instance.GetSpawnPoint() : null;

        if (spawn == null) spawn = respawnPoint;

        if (spawn == null)
        {
            Debug.LogWarning("PlayerRespawnHandler: No hay punto de respawn disponible.");
            return;
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        bool reEnableCC = false;
        if (characterController && characterController.enabled)
        {
            characterController.enabled = false;
            reEnableCC = true;
        }

        transform.position = spawn.position;
        if (alignRotationToSpawn) transform.rotation = spawn.rotation;

        if (reEnableCC) characterController.enabled = true;

        Debug.Log($"Jugador respawneado en: {spawn.name}");
    }

    private void OnPlayerRespawned()
    {
        AnchorMother boss = FindObjectOfType<AnchorMother>();
        if (boss != null)
        {
            boss.RestoreFullHealth();
        }

        BossHealth bosslvl1 = FindObjectOfType<BossHealth>();
        if (bosslvl1 != null)
        {
            bosslvl1.currentHP = maxHP;
        }

        AmmoPickup[] allAmmoPickups = FindObjectsOfType<AmmoPickup>();
        foreach (AmmoPickup pickup in allAmmoPickups)
        {
            pickup.Respawn();
        }

        TurretInteractable turret = FindObjectOfType<TurretInteractable>();
        if (turret != null)
        {
            turret.progress = progress;
        }

        BeltSwitch belt = FindObjectOfType<BeltSwitch>();
        if (belt != null)
        {
            belt.canActivate = false;
        }

        Debug.Log($"PlayerRespawnHandler: Boss restaurado y {allAmmoPickups.Length} municiones reaparecieron.");
    }
}
