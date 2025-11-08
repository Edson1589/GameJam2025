using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BossProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] private int damage = 25;
    [SerializeField] private float hitForce = 5f; // (opcional) empuje al jugador

    private Vector3 velocity;
    private Rigidbody rb;

    public void Init(Vector3 dir, float speed)
    {
        velocity = dir.normalized * speed;

        // Si tiene Rigidbody, aplicar la velocidad directamente
        if (rb != null)
        {
            rb.velocity = velocity;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Si no tiene Rigidbody, lo agregamos para usar f�sicas
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // Asegurar que tenga collider con trigger activo
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Update()
    {
        // Solo mover manualmente si no se usa Rigidbody
        if (rb == null)
        {
            transform.position += velocity * Time.deltaTime;
        }

        // Contador de vida del proyectil
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);

                // Peque�o empuje al jugador (si tiene Rigidbody)
                Rigidbody playerRb = other.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    playerRb.AddForce(velocity.normalized * hitForce, ForceMode.Impulse);
                }
            }

            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            Destroy(gameObject);
        }
    }
}
