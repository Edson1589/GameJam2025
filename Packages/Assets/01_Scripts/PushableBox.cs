using UnityEngine;

public class PushableBox : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerController nearbyPlayer;

    [Header("Settings")]
    [SerializeField] private float detectionRadius = 2f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Empieza como kinematic (no se puede empujar)
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        // Buscar al jugador cercano
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);

        bool playerWithArmsNearby = false;

        foreach (Collider col in colliders)
        {
            PlayerController player = col.GetComponent<PlayerController>();
            if (player != null)
            {
                nearbyPlayer = player;

                // Solo permitir empujar si tiene brazos
                if (player.hasArms)
                {
                    playerWithArmsNearby = true;
                    break;
                }
            }
        }

        // Activar/desactivar física según si hay jugador con brazos
        if (rb != null)
        {
            rb.isKinematic = !playerWithArmsNearby;
        }
    }

    // Visualizar radio de detección en editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}