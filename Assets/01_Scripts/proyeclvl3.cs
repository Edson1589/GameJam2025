using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class proyeclvl3 : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] private int damage = 25;
    [SerializeField] private float hitForce = 5f;
    [SerializeField] private float baseSpeed = 10f;

    [Header("Homing (Seguimiento)")]
    [SerializeField] private bool enableHoming = true;
    [SerializeField] private float homingStrength = 2f;
    [SerializeField] private float homingStartDelay = 0.5f; // Tiempo antes de empezar a seguir
    [SerializeField] private float maxHomingAngle = 45f; // Ángulo máximo de giro por frame
    private float homingTimer = 0f;

    [Header("Explosion")]
    [SerializeField] private bool explodeOnImpact = false;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private int explosionDamage = 15;
    [SerializeField] private GameObject explosionEffect;

    [Header("Split Projectile")]
    [SerializeField] private bool splitOnImpact = false;
    [SerializeField] private int splitCount = 3;
    [SerializeField] private GameObject splitProjectilePrefab;
    [SerializeField] private float splitSpreadAngle = 30f;

    [Header("Speed Variation")]
    [SerializeField] private bool accelerateOverTime = true;
    [SerializeField] private float accelerationRate = 1.5f;
    [SerializeField] private float maxSpeed = 20f;

    [Header("Visual/Audio")]
    [SerializeField] private GameObject trailEffect;
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float impactSoundVolume = 0.7f;
    private AudioSource audioSource;

    private Vector3 velocity;
    private Rigidbody rb;
    private Transform playerTransform;
    private Transform ownerRoot; // Para evitar colisiones con el dueño
    private float currentSpeed;
    private bool hasHit = false;

    public void Init(Vector3 dir, float speed)
    {
        velocity = dir.normalized * speed;
        baseSpeed = speed;
        currentSpeed = speed;

        if (rb != null)
        {
            rb.velocity = velocity;
        }
    }

    // Sobrecarga para incluir el owner (para evitar colisiones con el jugador)
    public void Init(Vector3 dir, float speed, Transform owner)
    {
        Init(dir, speed);
        ownerRoot = owner;
        Debug.Log($"proyeclvl3: Inicializado con owner: {owner?.name}, velocidad: {speed}, dirección: {dir}");
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;

        // Buscar jugador
        FindPlayer();

        // Configurar audio
        if (impactSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }
    }

    private void Start()
    {
        if (trailEffect != null)
        {
            GameObject trail = Instantiate(trailEffect, transform);
            trail.transform.localPosition = Vector3.zero;
        }
    }

    private void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    private void Update()
    {
        if (hasHit) return;

        homingTimer += Time.deltaTime;
        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0f)
        {
            Debug.LogWarning($"proyeclvl3: Tiempo de vida agotado. Destruyendo proyectil.");
            DestroyProjectile();
            return;
        }

        // Acelerar con el tiempo
        if (accelerateOverTime && currentSpeed < maxSpeed)
        {
            currentSpeed = Mathf.Min(currentSpeed + accelerationRate * Time.deltaTime, maxSpeed);
        }

        // Actualizar movimiento
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        if (rb == null)
        {
            transform.position += velocity * Time.deltaTime;
            return;
        }

        // Sistema de seguimiento (Homing)
        if (enableHoming && homingTimer >= homingStartDelay && playerTransform != null)
        {
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            Vector3 currentDirection = velocity.normalized;

            // Calcular el ángulo entre la dirección actual y hacia el jugador
            float angleToPlayer = Vector3.Angle(currentDirection, directionToPlayer);

            if (angleToPlayer > 0.1f && angleToPlayer <= maxHomingAngle * 2f)
            {
                // Interpolar suavemente hacia el jugador
                Vector3 newDirection = Vector3.Slerp(currentDirection, directionToPlayer, homingStrength * Time.deltaTime);
                velocity = newDirection * currentSpeed;
            }
            else if (angleToPlayer <= maxHomingAngle)
            {
                // Si está cerca del ángulo máximo, seguir directamente
                velocity = directionToPlayer * currentSpeed;
            }

            rb.velocity = velocity;
        }
        else
        {
            // Mantener velocidad actual pero con aceleración
            velocity = velocity.normalized * currentSpeed;
            rb.velocity = velocity;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // Ignorar colisión con el dueño del proyectil
        if (ownerRoot != null && other.transform.root == ownerRoot)
        {
            Debug.Log($"proyeclvl3: Ignorando colisión con owner: {other.name}");
            return;
        }

        Debug.Log($"proyeclvl3: Colisión detectada con: {other.name}, tag: {other.tag}, layer: {other.gameObject.layer}");

        if (other.CompareTag("Player"))
        {
            HitPlayer(other);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            HitEnvironment(other);
        }
    }

    private void HitPlayer(Collider playerCollider)
    {
        hasHit = true;

        PlayerHealth health = playerCollider.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        Rigidbody playerRb = playerCollider.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.AddForce(velocity.normalized * hitForce, ForceMode.Impulse);
        }

        PlayImpactSound();

        if (explodeOnImpact)
        {
            Explode();
        }
        else if (splitOnImpact)
        {
            SplitProjectile();
        }
        else
        {
            DestroyProjectile();
        }
    }

    private void HitEnvironment(Collider environmentCollider)
    {
        hasHit = true;

        if (explodeOnImpact)
        {
            Explode();
        }
        else if (splitOnImpact)
        {
            SplitProjectile();
        }
        else
        {
            DestroyProjectile();
        }
    }

    private void Explode()
    {
        // Daño en área
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                PlayerHealth health = col.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    // Daño reducido por distancia
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    float damageMultiplier = 1f - (distance / explosionRadius);
                    int finalDamage = Mathf.RoundToInt(explosionDamage * damageMultiplier);
                    health.TakeDamage(finalDamage);
                }

                // Empuje por explosión
                Rigidbody playerRb = col.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Vector3 explosionDirection = (col.transform.position - transform.position).normalized;
                    playerRb.AddForce(explosionDirection * hitForce * 1.5f, ForceMode.Impulse);
                }
            }
        }

        // Efecto visual de explosión
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosion, 2f);
        }

        PlayImpactSound();
        DestroyProjectile();
    }

    private void SplitProjectile()
    {
        if (splitProjectilePrefab == null || splitCount <= 0) return;

        // Calcular ángulo de separación
        float angleStep = splitSpreadAngle / (splitCount - 1);
        float startAngle = -splitSpreadAngle / 2f;

        for (int i = 0; i < splitCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 splitDirection = rotation * velocity.normalized;

            GameObject splitProjectile = Instantiate(splitProjectilePrefab, transform.position, Quaternion.identity);
            proyeclvl3 splitScript = splitProjectile.GetComponent<proyeclvl3>();
            
            if (splitScript != null)
            {
                // Los proyectiles divididos no se dividen de nuevo y tienen menos daño
                splitScript.splitOnImpact = false;
                splitScript.damage = Mathf.Max(1, damage / 2);
                splitScript.Init(splitDirection, currentSpeed * 0.8f);
            }
        }

        PlayImpactSound();
        DestroyProjectile();
    }

    private void PlayImpactSound()
    {
        if (impactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(impactSound, impactSoundVolume);
        }
    }

    private void DestroyProjectile()
    {
        // Desactivar visualmente antes de destruir para que el sonido se reproduzca
        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().enabled = false;
        }
        if (GetComponent<Collider>() != null)
        {
            GetComponent<Collider>().enabled = false;
        }

        // Destruir después de un pequeño delay para que el sonido se reproduzca
        Destroy(gameObject, 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar radio de explosión
        if (explodeOnImpact)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }

        // Visualizar dirección del proyectil
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, velocity.normalized * 2f);
    }
}
