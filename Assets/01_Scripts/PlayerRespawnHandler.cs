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
    }
}
