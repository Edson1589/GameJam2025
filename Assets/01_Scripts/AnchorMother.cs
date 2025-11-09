using UnityEngine;
using UnityEngine.SceneManagement;

public class AnchorMother : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private int totalPanelsToActivate = 4;
    [SerializeField] private Transform[] panelPositions;
    
    [Header("Boss Health")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP;
    [SerializeField] private int damagePerBullet = 10;
    [SerializeField] private bool canTakeDamageFromPlayer = true;

    [Header("Visual")]
    [SerializeField] private GameObject coreVisual;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material damagedMaterial;
    [SerializeField] private float rotationSpeed = 20f;
    
    [Header("Boss Rotation")]
    [SerializeField] private bool rotateTowardsPlayer = true;
    [SerializeField] private float rotationSpeedTowardsPlayer = 5f;
    [SerializeField] private bool onlyRotateY = true;
    [SerializeField] private Transform rotationTarget;
    [SerializeField] private Vector3 fixedRotation = new Vector3(-90f, 0f, 0f);
    [SerializeField] private float yRotationOffset = 0f;

    [Header("Boss Movement")]
    [SerializeField] private bool enableMovement = true;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float hoverAmplitude = 0.5f;
    [SerializeField] private float hoverSpeed = 2f;
    [SerializeField] private float minDistanceToPlayer = 5f;
    [SerializeField] private float maxDistanceToPlayer = 15f;
    [SerializeField] private bool followPlayer = false;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private bool usePatrol = false;
    private int currentPatrolIndex = 0;
    private Vector3 startPosition;
    private float hoverOffset = 0f;

    [Header("Magnetic Field")]
    [SerializeField] private float fieldRange = 15f;
    [SerializeField] private float pullForce = 15f;
    [SerializeField] private bool fieldActive = true;

    [Header("Victory")]
    [SerializeField] private string videoSceneName = "VideoSceneFinal";
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField] private bool useVideoScene = true;

    [Header("Shooting")]
    [SerializeField] private Transform[] firePoints; 
    private int currentFireIndex = 0;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float shootStartDelay = 20f;
    private float shootStartTimer = 0f;
    private float shootTimer = 0f;
    [SerializeField] private bool predictiveAiming = true; // Apuntar donde estará el jugador

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;
    [SerializeField, Range(0f, 1f)] private float shootSoundVolume = 0.7f;

    private int panelsActivated = 0;
    private bool isBossDefeated = false;
    private Renderer coreRenderer;
    private Transform playerTransform;
    private Rigidbody rb;

    void Start()
    {
        InitializeBoss();
    }

    private void InitializeBoss()
    {
        if (coreVisual != null)
        {
            coreRenderer = coreVisual.GetComponent<Renderer>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 50f;
        }

        currentHP = maxHP;
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();

        if (rotateTowardsPlayer && onlyRotateY && rb != null)
        {
            rb.constraints |= (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ);
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationY;
        }

        UpdateVisual();
    }

    void Update()
    {
        if (isBossDefeated) return;

        UpdateCoreVisual();
        FindPlayerIfNeeded();
        HandleShooting();
    }

    void FixedUpdate()
    {
        if (isBossDefeated) return;

        if (rotateTowardsPlayer && playerTransform != null)
        {
            RotateTowardsPlayer();
        }

        if (enableMovement)
        {
            HandleMovement();
        }

        if (fieldActive && playerTransform != null)
        {
            ApplyMagneticField();
        }
    }

    private void UpdateCoreVisual()
    {
        if (coreVisual != null)
        {
            coreVisual.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            coreVisual.transform.Rotate(Vector3.forward, rotationSpeed * 0.5f * Time.deltaTime);
        }
    }

    private void FindPlayerIfNeeded()
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

    private void HandleMovement()
    {
        if (playerTransform == null) return;

        Vector3 targetPosition = startPosition;

        // Movimiento flotante (hovering)
        hoverOffset += hoverSpeed * Time.fixedDeltaTime;
        float verticalOffset = Mathf.Sin(hoverOffset) * hoverAmplitude;
        targetPosition.y = startPosition.y + verticalOffset;

        // Seguir al jugador o patrullar
        if (followPlayer && !usePatrol)
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0f;
            float distance = directionToPlayer.magnitude;

            if (distance > maxDistanceToPlayer)
            {
                // Acercarse si está muy lejos
                targetPosition.x = transform.position.x + directionToPlayer.normalized.x * moveSpeed * Time.fixedDeltaTime;
                targetPosition.z = transform.position.z + directionToPlayer.normalized.z * moveSpeed * Time.fixedDeltaTime;
            }
            else if (distance < minDistanceToPlayer)
            {
                // Alejarse si está muy cerca
                targetPosition.x = transform.position.x - directionToPlayer.normalized.x * moveSpeed * Time.fixedDeltaTime;
                targetPosition.z = transform.position.z - directionToPlayer.normalized.z * moveSpeed * Time.fixedDeltaTime;
            }
            else
            {
                // Mantener distancia
                targetPosition.x = transform.position.x;
                targetPosition.z = transform.position.z;
            }
        }
        else if (usePatrol && patrolPoints != null && patrolPoints.Length > 0)
        {
            // Patrullaje entre puntos
            Transform targetPoint = patrolPoints[currentPatrolIndex];
            if (targetPoint != null)
            {
                Vector3 direction = (targetPoint.position - transform.position);
                direction.y = 0f;
                
                if (direction.magnitude > 0.5f)
                {
                    targetPosition.x = transform.position.x + direction.normalized.x * moveSpeed * Time.fixedDeltaTime;
                    targetPosition.z = transform.position.z + direction.normalized.z * moveSpeed * Time.fixedDeltaTime;
                }
                else
                {
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                }
            }
        }

        // Aplicar movimiento
        if (rb != null && rb.isKinematic)
        {
            rb.MovePosition(targetPosition);
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    private void RotateTowardsPlayer()
    {
        if (playerTransform == null) return;

        Transform targetTransform = rotationTarget != null ? rotationTarget : transform;
        Vector3 targetPosition = targetTransform.position;
        Vector3 directionToPlayer = playerTransform.position - targetPosition;
        
        if (onlyRotateY)
        {
            directionToPlayer.y = 0f;
        }

        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation;
            
            if (onlyRotateY)
            {
                float angleRadians = Mathf.Atan2(directionToPlayer.x, directionToPlayer.z);
                float targetY = angleRadians * Mathf.Rad2Deg + yRotationOffset;
                
                while (targetY < 0) targetY += 360f;
                while (targetY >= 360) targetY -= 360f;
                
                targetRotation = Quaternion.Euler(fixedRotation.x, targetY, fixedRotation.z);
            }
            else
            {
                targetRotation = Quaternion.LookRotation(directionToPlayer.normalized);
            }
            
            if (targetTransform == transform)
            {
                if (rb != null && rb.isKinematic)
                {
                    rb.constraints |= (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ);
                    rb.constraints &= ~RigidbodyConstraints.FreezeRotationY;
                    rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeedTowardsPlayer * Time.fixedDeltaTime));
                }
                else
                {
                    targetTransform.rotation = Quaternion.Slerp(targetTransform.rotation, targetRotation, rotationSpeedTowardsPlayer * Time.fixedDeltaTime);
                }
            }
            else
            {
                targetTransform.rotation = Quaternion.Slerp(targetTransform.rotation, targetRotation, rotationSpeedTowardsPlayer * Time.fixedDeltaTime);
            }
        }
    }

    private void HandleShooting()
    {
        if (!fieldActive || projectilePrefab == null || firePoints.Length < 2) return;

        shootStartTimer += Time.deltaTime;

        if (shootStartTimer >= shootStartDelay)
        {
            shootTimer += Time.deltaTime;
            if (shootTimer >= shootInterval)
            {
                ShootProjectile();
                shootTimer = 0f;
            }
        }
    }

    private void ShootProjectile()
    {
        if (projectilePrefab == null || firePoints.Length < 2 || playerTransform == null) return;

        Transform firePoint = firePoints[currentFireIndex];
        currentFireIndex = (currentFireIndex + 1) % firePoints.Length;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        if (projectileRb != null)
        {
            Vector3 direction;
            
            if (predictiveAiming)
            {
                // Apuntar donde estará el jugador (predicción básica)
                Vector3 playerVelocity = Vector3.zero;
                Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    playerVelocity = playerRb.velocity;
                }
                
                float timeToReach = Vector3.Distance(firePoint.position, playerTransform.position) / projectileSpeed;
                Vector3 predictedPosition = playerTransform.position + playerVelocity * timeToReach;
                direction = (predictedPosition - firePoint.position).normalized;
            }
            else
            {
                direction = (playerTransform.position - firePoint.position).normalized;
            }

            projectileRb.velocity = direction * projectileSpeed;
        }

        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound, shootSoundVolume);
        }
    }

    private void ApplyMagneticField()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance > fieldRange) return;

        Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();

        if (playerRb != null)
        {
            Vector3 pullDirection = (transform.position - playerTransform.position).normalized;
            float resistance = playerController != null ? playerController.GetMagneticResistance() : 0f;
            float effectivePull = pullForce * (1f - resistance);
            playerRb.AddForce(pullDirection * effectivePull, ForceMode.Force);
        }
    }

    public void RegisterPanelActivation()
    {
        panelsActivated++;
        UpdateVisual();

        if (panelsActivated >= totalPanelsToActivate)
        {
            DeactivateField();
        }
    }

    private void DeactivateField()
    {
        fieldActive = false;

        if (coreRenderer != null && damagedMaterial != null)
        {
            coreRenderer.material = damagedMaterial;
        }
    }

    private void UpdateVisual()
    {
        if (coreRenderer == null) return;

        float damagePercent = (float)panelsActivated / totalPanelsToActivate;

        if (normalMaterial != null)
        {
            Color currentColor = Color.Lerp(normalMaterial.color, Color.red, damagePercent);
            if (coreRenderer.material != null)
            {
                coreRenderer.material.color = currentColor;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isBossDefeated) return;

        if (canTakeDamageFromPlayer)
        {
            BulletProjectile bullet = other.GetComponent<BulletProjectile>();
            if (bullet != null)
            {
                TakeDamage(damagePerBullet);
                Destroy(bullet.gameObject);
                return;
            }
        }

        if (other.CompareTag("Player"))
        {
            if (!fieldActive)
            {
                DefeatBoss();
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (isBossDefeated || !canTakeDamageFromPlayer || amount <= 0) return;

        currentHP = Mathf.Max(0, currentHP - amount);
        UpdateHealthVisual();

        if (currentHP <= 0)
        {
            DefeatBoss();
        }
    }

    // Métodos públicos para la UI
    public int GetCurrentHP() => currentHP;
    public int GetMaxHP() => maxHP;
    public float GetHealthPercent() => maxHP > 0 ? (float)currentHP / maxHP : 0f;

    private void UpdateHealthVisual()
    {
        if (coreRenderer == null) return;

        float healthPercent = (float)currentHP / maxHP;
        Color healthColor;

        if (healthPercent > 0.6f)
        {
            healthColor = Color.Lerp(Color.yellow, Color.green, (healthPercent - 0.6f) / 0.4f);
        }
        else if (healthPercent > 0.3f)
        {
            healthColor = Color.Lerp(Color.red, Color.yellow, (healthPercent - 0.3f) / 0.3f);
        }
        else
        {
            healthColor = Color.red;
        }

        if (coreRenderer.material != null)
        {
            coreRenderer.material.color = healthColor;
        }
    }

    private void DefeatBoss()
    {
        isBossDefeated = true;

        if (coreVisual != null)
        {
            coreVisual.SetActive(false);
        }

        Invoke("LoadVictoryScene", 2f);
    }

    private void LoadVictoryScene()
    {
        // En Level 3, siempre ir a VideoSceneFinal cuando muere el boss
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"AnchorMother: Cargando escena de victoria. Escena actual: {currentScene}");
        
        if (currentScene.Contains("Level_01_Ensamblee 3") || currentScene.Contains("Level3") || currentScene.Contains("Ensamblee 3"))
        {
            Debug.Log("AnchorMother: Detectado Level 3, cargando VideoSceneFinal");
            SceneManager.LoadScene("VideoSceneFinal");
            return;
        }

        if (useVideoScene && !string.IsNullOrEmpty(videoSceneName))
        {
            Debug.Log($"AnchorMother: Cargando escena de video: {videoSceneName}");
            SceneManager.LoadScene(videoSceneName);
        }
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"AnchorMother: Cargando siguiente escena: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("AnchorMother: No se pudo determinar qué escena cargar. Verifica la configuración.");
        }
    }

    // Método público para restaurar la salud del boss (cuando el jugador respawnea)
    public void RestoreFullHealth()
    {
        currentHP = maxHP;
        isBossDefeated = false;
        UpdateHealthVisual();
        
        if (coreVisual != null)
        {
            coreVisual.SetActive(true);
        }
        
        Debug.Log($"AnchorMother: Salud restaurada a {currentHP}/{maxHP}");
    }

    private void OnDrawGizmosSelected()
    {
        // Campo magnético
        Gizmos.color = fieldActive ? new Color(1f, 0f, 0f, 0.3f) : new Color(0.5f, 0.5f, 0.5f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, fieldRange);

        // Paneles
        if (panelPositions != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform panel in panelPositions)
            {
                if (panel != null)
                {
                    Gizmos.DrawLine(transform.position, panel.position);
                    Gizmos.DrawWireCube(panel.position, Vector3.one * 0.5f);
                }
            }
        }

        // Puntos de patrulla
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.5f);
                    int next = (i + 1) % patrolPoints.Length;
                    if (patrolPoints[next] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[next].position);
                    }
                }
            }
        }
    }
}
