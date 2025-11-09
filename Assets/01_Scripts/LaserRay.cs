using UnityEngine;
using System.Collections;

public class LaserRay : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform firePoint;

    [Header("Proyectil")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 40f;
    [SerializeField] private float projectileLifetime = 3f;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 0.25f;
    private float nextFireTime = 0f;

    [Header("Desbloqueo")]
    [SerializeField] private bool requiresTorso = true;
    [SerializeField] private bool unlocked = false;

    [Header("Ammo System")]
    [SerializeField] private bool requiresAmmo = true;
    [SerializeField] private int ammoPerShot = 1;

    [Header("Input")]
    [SerializeField] private KeyCode shootKey = KeyCode.K;
    [SerializeField] private bool useInputManager = false;

    [Header("SFX / VFX (opcionales)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip sfxShoot;
    [SerializeField] private GameObject muzzleVFX;
    [SerializeField] private float muzzleVFXLife = 0.5f;

    [SerializeField] private Transform camTransform;

    private void Awake()
    {
        if (!sfxSource) sfxSource = GetComponent<AudioSource>();
        if (!camTransform && Camera.main) camTransform = Camera.main.transform;
    }

    private void Update()
    {
        // Verificar input de disparo
        bool shootInput = false;
        
        if (useInputManager && InputManager.Instance != null)
        {
            // Intentar usar InputManager si está disponible
            shootInput = InputManager.Instance.GetKeyDown("Shoot");
        }
        else
        {
            // Usar input directo con la tecla K
            shootInput = Input.GetKeyDown(shootKey);
        }

        // Debug del input
        if (Input.GetKeyDown(shootKey))
        {
            Debug.Log($"LaserRay: Tecla {shootKey} presionada. shootInput: {shootInput}, unlocked: {unlocked}, requiresTorso: {requiresTorso}");
        }

        if (shootInput)
        {
            TryFire();
        }
    }

    public void SetUnlocked(bool on) => unlocked = on;
    public bool IsUnlocked => unlocked;
    public bool IsReady() => Time.time >= nextFireTime;

    public float Cooldown01()
    {
        if (cooldown <= 0f) return 1f;
        float remaining = nextFireTime - Time.time;
        return Mathf.Clamp01(1f - Mathf.Clamp01(remaining / cooldown));
    }

    public void TryFire()
    {
        // Debug: verificar por qué no dispara
        if (!IsReady())
        {
            Debug.LogWarning("LaserRay: No está listo (cooldown activo)");
            return;
        }
        
        if (requiresTorso && !unlocked)
        {
            Debug.LogWarning($"LaserRay: Requiere torso y no está desbloqueado. requiresTorso: {requiresTorso}, unlocked: {unlocked}");
            return;
        }
        
        if (!projectilePrefab)
        {
            Debug.LogError("LaserRay: No hay projectilePrefab asignado!");
            return;
        }
        
        if (!firePoint)
        {
            Debug.LogError("LaserRay: No hay firePoint asignado!");
            return;
        }

        // Verificar munición si es requerida
        if (requiresAmmo)
        {
            PlayerAmmoSystem ammoSystem = PlayerAmmoSystem.Instance;
            if (ammoSystem == null)
            {
                Debug.LogWarning("LaserRay: PlayerAmmoSystem no encontrado. ¿Está agregado al jugador?");
                return;
            }
            
            if (!ammoSystem.CanShoot(ammoPerShot))
            {
                Debug.LogWarning($"LaserRay: No hay munición suficiente. Actual: {ammoSystem.CurrentAmmo}, Necesario: {ammoPerShot}");
                return;
            }
        }

        Debug.Log("LaserRay: Disparando!");
        Fire();
        nextFireTime = Time.time + cooldown;
    }

    private void Fire()
    {
        // Consumir munición si es requerida
        if (requiresAmmo)
        {
            PlayerAmmoSystem ammoSystem = PlayerAmmoSystem.Instance;
            if (ammoSystem != null)
            {
                if (!ammoSystem.ConsumeAmmo(ammoPerShot))
                {
                    Debug.LogWarning("LaserRay: No se pudo consumir munición");
                    return;
                }
                Debug.Log($"LaserRay: Munición consumida. Restante: {ammoSystem.CurrentAmmo}");
            }
        }

        Vector3 origin = firePoint.position;
        Vector3 dir = (camTransform ? camTransform.forward : firePoint.forward);

        Debug.Log($"LaserRay: Instanciando proyectil en {origin}, dirección: {dir}, velocidad: {projectileSpeed}");

        GameObject go = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(dir, Vector3.up));
        
        if (go == null)
        {
            Debug.LogError("LaserRay: No se pudo instanciar el proyectil!");
            return;
        }

        // Asegurar que el objeto esté activo
        go.SetActive(true);
        
        Debug.Log($"LaserRay: Proyectil instanciado: {go.name}, activo: {go.activeSelf}");

        // Buscar scripts en el objeto y sus hijos (con más detalle)
        // Primero intentar con BulletProjectile
        var bullet = go.GetComponent<BulletProjectile>();
        if (bullet == null)
        {
            bullet = go.GetComponentInChildren<BulletProjectile>(true); // Incluir inactivos
        }
        
        if (bullet != null)
        {
            bullet.Init(transform.root, dir, projectileSpeed, projectileLifetime);
            Debug.Log($"LaserRay: Proyectil inicializado como BulletProjectile. Velocidad: {dir.normalized * projectileSpeed}, Vida: {projectileLifetime}s");
        }
        else
        {
            // Intentar con PlayerBossBullet
            var playerBullet = go.GetComponent<PlayerBossBullet>();
            if (playerBullet == null)
            {
                playerBullet = go.GetComponentInChildren<PlayerBossBullet>(true);
            }
            
            if (playerBullet != null)
            {
                playerBullet.Init(transform.root, dir, projectileSpeed, projectileLifetime);
                Debug.Log($"LaserRay: Proyectil inicializado como PlayerBossBullet. Velocidad: {dir.normalized * projectileSpeed}, Vida: {projectileLifetime}s");
            }
            else
            {
                // Intentar con proyeclvl3 (proyectil del boss, pero puede usarse para el jugador)
                var proyeclvl3 = go.GetComponent<proyeclvl3>();
                if (proyeclvl3 == null)
                {
                    proyeclvl3 = go.GetComponentInChildren<proyeclvl3>(true);
                }
                
                if (proyeclvl3 != null)
                {
                    // Usar la sobrecarga que incluye el owner para evitar colisiones con el jugador
                    proyeclvl3.Init(dir, projectileSpeed, transform.root);
                    // proyeclvl3 maneja su propio lifetime, pero podemos ajustarlo si es necesario
                    Debug.Log($"LaserRay: Proyectil inicializado como proyeclvl3. Velocidad: {dir.normalized * projectileSpeed}, Owner: {transform.root.name}");
                }
                else
                {
                    // Intentar con BossProjectile
                    var bossProjectile = go.GetComponent<BossProjectile>();
                    if (bossProjectile == null)
                    {
                        bossProjectile = go.GetComponentInChildren<BossProjectile>(true);
                    }
                    
                    if (bossProjectile != null)
                    {
                        // Usar la sobrecarga que incluye el owner para evitar colisiones con el jugador
                        bossProjectile.Init(dir, projectileSpeed, transform.root);
                        Debug.Log($"LaserRay: Proyectil inicializado como BossProjectile. Velocidad: {dir.normalized * projectileSpeed}, Owner: {transform.root.name}");
                    }
                    else
                    {
                        Debug.LogWarning("LaserRay: No se encontró BulletProjectile, PlayerBossBullet, proyeclvl3 ni BossProjectile. Componentes encontrados:");
                        var allComponents = go.GetComponents<MonoBehaviour>();
                        foreach (var comp in allComponents)
                        {
                            Debug.LogWarning($"  - {comp.GetType().Name}");
                        }
                        
                        var rb = go.GetComponent<Rigidbody>();
                        if (rb)
                        {
                            // Asegurar que el Rigidbody esté configurado correctamente
                            rb.useGravity = false;
                            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                            
                            // Aplicar velocidad
                            rb.velocity = dir.normalized * projectileSpeed;
                            
                            Debug.Log($"LaserRay: Proyectil configurado con Rigidbody. Velocidad: {rb.velocity}, Magnitud: {rb.velocity.magnitude}");
                            Debug.Log($"LaserRay: Posición inicial: {go.transform.position}, Destrucción en: {projectileLifetime}s");
                            
                            Destroy(go, projectileLifetime);
                        }
                        else
                        {
                            Debug.LogWarning("LaserRay: El proyectil no tiene ningún script de proyectil conocido ni Rigidbody!");
                            // Intentar agregar Rigidbody si no tiene
                            rb = go.AddComponent<Rigidbody>();
                            rb.useGravity = false;
                            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                            rb.velocity = dir.normalized * projectileSpeed;
                            Debug.Log("LaserRay: Rigidbody agregado al proyectil");
                            Destroy(go, projectileLifetime);
                        }
                    }
                }
            }
        }

        // Verificar si el proyectil tiene renderer
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer == null)
        {
            renderer = go.GetComponentInChildren<Renderer>();
        }
        if (renderer == null)
        {
            Debug.LogWarning("LaserRay: El proyectil no tiene Renderer! No se verá visualmente.");
        }
        else
        {
            Debug.Log($"LaserRay: Proyectil tiene Renderer: {renderer.name}, enabled: {renderer.enabled}, scale: {go.transform.localScale}");
        }

        // Verificar Rigidbody
        Rigidbody projRb = go.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            Debug.Log($"LaserRay: Rigidbody - useGravity: {projRb.useGravity}, isKinematic: {projRb.isKinematic}, velocity: {projRb.velocity}");
        }

        // Verificar si el objeto sigue existiendo después de un frame
        StartCoroutine(CheckProjectileAfterFrame(go));

        if (muzzleVFX)
        {
            var fx = Instantiate(muzzleVFX, firePoint.position, firePoint.rotation, firePoint);
            Destroy(fx, muzzleVFXLife);
        }
        if (sfxSource && sfxShoot) sfxSource.PlayOneShot(sfxShoot);
    }

    private IEnumerator CheckProjectileAfterFrame(GameObject projectile)
    {
        yield return null; // Esperar un frame
        
        if (projectile == null)
        {
            Debug.LogError("LaserRay: El proyectil fue destruido inmediatamente después de crearse!");
        }
        else
        {
            Debug.Log($"LaserRay: Proyectil sigue existiendo después de 1 frame. Posición: {projectile.transform.position}, Activo: {projectile.activeSelf}");
        }
    }

}
