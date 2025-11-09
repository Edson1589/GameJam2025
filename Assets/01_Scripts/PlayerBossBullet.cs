using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerBossBullet : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 40f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private int damage = 10;
    [SerializeField] private bool destroyOnAnyNonTrigger = true;

    [Header("Visual Effects")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject trailEffect;
    [SerializeField] private bool rotateTowardsDirection = true;

    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip fireSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 0.7f;
    private AudioSource audioSource;

    [Header("Advanced")]
    [SerializeField] private bool canPierce = false; // Atraviesa enemigos
    [SerializeField] private int maxPierceCount = 1;
    [SerializeField] private bool destroyOnBossHit = true;
    private int pierceCount = 0;

    private Vector3 velocity;
    private Transform ownerRoot;
    private float life;
    private bool hasHit = false;

    /// <summary>
    /// Inicializa el proyectil con dirección, velocidad y duración
    /// </summary>
    public void Init(Transform ownerRoot, Vector3 direction, float projectileSpeed, float lifetime = -1f)
    {
        this.ownerRoot = ownerRoot;
        this.velocity = direction.normalized * projectileSpeed;
        this.speed = projectileSpeed;
        this.life = lifetime > 0f ? lifetime : lifeTime;

        // Aplicar velocidad al Rigidbody si existe
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = velocity;
        }

        // Rotar hacia la dirección de movimiento
        if (rotateTowardsDirection && direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Reproducir sonido de disparo
        PlaySound(fireSound);
    }

    private void Awake()
    {
        // Configurar collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Configurar Rigidbody si no existe
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // Configurar audio
        if (hitSound != null || fireSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 50f;
        }

        // Inicializar vida si no se ha hecho
        if (life <= 0f)
        {
            life = lifeTime;
        }
    }

    private void Start()
    {
        // Crear efecto de estela si existe
        if (trailEffect != null)
        {
            GameObject trail = Instantiate(trailEffect, transform);
            trail.transform.localPosition = Vector3.zero;
        }
    }

    private void Update()
    {
        if (hasHit && !canPierce) return;

        // Movimiento del proyectil
        if (GetComponent<Rigidbody>() == null)
        {
            transform.position += velocity * Time.deltaTime;
        }

        // Actualizar rotación hacia la dirección de movimiento
        if (rotateTowardsDirection && velocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(velocity.normalized);
        }

        // Contador de vida
        life -= Time.deltaTime;
        if (life <= 0f)
        {
            DestroyProjectile();
        }
    }

    private void FixedUpdate()
    {
        // Actualizar velocidad del Rigidbody si cambió
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && rb.velocity != velocity)
        {
            rb.velocity = velocity;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit && !canPierce) return;

        // Ignorar colisiones con el dueño del proyectil
        if (ownerRoot != null && other.transform.root == ownerRoot)
        {
            return;
        }

        // Detectar jefe AnchorMother
        AnchorMother boss = other.GetComponent<AnchorMother>() ?? other.GetComponentInParent<AnchorMother>();
        if (boss != null)
        {
            HitBoss(boss);
            return;
        }

        // Detectar otros enemigos si el proyectil puede atravesar
        if (canPierce)
        {
            PusherBot bot = other.GetComponent<PusherBot>() ?? other.GetComponentInParent<PusherBot>();
            if (bot != null)
            {
                bot.KillByLaser();
                pierceCount++;
                if (pierceCount >= maxPierceCount)
                {
                    DestroyProjectile();
                }
                return;
            }
        }

        // Colisión con el entorno
        if (!other.isTrigger && destroyOnAnyNonTrigger)
        {
            HitEnvironment(other);
        }
    }

    private void HitBoss(AnchorMother boss)
    {
        if (boss == null) return;

        // Aplicar daño al boss
        boss.TakeDamage(damage);

        // Efecto visual de impacto
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // Sonido de impacto
        PlaySound(hitSound);

        // Destruir o continuar según configuración
        if (destroyOnBossHit)
        {
            hasHit = true;
            DestroyProjectile();
        }
        else if (canPierce)
        {
            pierceCount++;
            if (pierceCount >= maxPierceCount)
            {
                DestroyProjectile();
            }
        }
    }

    private void HitEnvironment(Collider environment)
    {
        // Efecto visual de impacto en el entorno
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // Sonido de impacto
        PlaySound(hitSound);

        hasHit = true;
        DestroyProjectile();
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, soundVolume);
        }
    }

    private void DestroyProjectile()
    {
        // Desactivar componentes visuales pero mantener el objeto un momento para el sonido
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Destruir después de un pequeño delay para que el sonido se reproduzca
        Destroy(gameObject, 0.2f);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar dirección del proyectil
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, velocity.normalized * 2f);
    }
}

