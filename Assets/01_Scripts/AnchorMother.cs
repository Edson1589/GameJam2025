using UnityEngine;
using UnityEngine.SceneManagement;

public class AnchorMother : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private int totalPanelsToActivate = 4;
    [SerializeField] private Transform[] panelPositions;

    [Header("Visual")]
    [SerializeField] private GameObject coreVisual;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material damagedMaterial;
    [SerializeField] private float rotationSpeed = 20f;

    [Header("Magnetic Field")]
    [SerializeField] private float fieldRange = 15f;
    [SerializeField] private float pullForce = 15f;
    [SerializeField] private bool fieldActive = true;

    [Header("Victory")]
    [SerializeField] private string nextSceneName = "MainMenu";

    [Header("Shooting")]
    [SerializeField] private Transform[] firePoints; 
    private int currentFireIndex = 0;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float projectileSpeed = 10f;

    [SerializeField] private float shootStartDelay = 20f;
    private float shootStartTimer = 0f;
    private float shootTimer = 0f;



    private int panelsActivated = 0;
    private bool isBossDefeated = false;
    private Renderer coreRenderer;
    private Transform playerTransform;

    void Start()
    {
        if (coreVisual != null)
        {
            coreRenderer = coreVisual.GetComponent<Renderer>();
        }

        UpdateVisual();

        Debug.Log($"=== BOSS FIGHT: ANCLA MADRE ===");
        Debug.Log($"Activa {totalPanelsToActivate} paneles para debilitar el campo magnético");
    }

    void Update()
    {
        if (isBossDefeated) return;

        // Rotar el núcleo
        if (coreVisual != null)
        {
            coreVisual.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            coreVisual.transform.Rotate(Vector3.forward, rotationSpeed * 0.5f * Time.deltaTime);
        }

        // Buscar al jugador si no lo tenemos
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        // Disparar proyectil si el campo está activo y ha pasado el tiempo de espera
        if (fieldActive && projectilePrefab != null && firePoints.Length >= 2)
        {
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



    }

    void FixedUpdate()
    {
        if (isBossDefeated || !fieldActive) return;

        // Aplicar campo magnético al jugador
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            if (distance <= fieldRange)
            {
                ApplyMagneticField();
            }
        }
    }

    private void ApplyMagneticField()
    {
        if (playerTransform == null) return;

        Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();

        if (playerRb != null)
        {
            Vector3 pullDirection = (transform.position - playerTransform.position).normalized;

            // Aplicar resistencia si el jugador tiene torso
            float resistance = 0f;
            if (playerController != null)
            {
                resistance = playerController.GetMagneticResistance();
            }

            float effectivePull = pullForce * (1f - resistance);
            playerRb.AddForce(pullDirection * effectivePull, ForceMode.Force);
        }
    }

    public void RegisterPanelActivation()
    {
        panelsActivated++;

        Debug.Log($"Panel activado! ({panelsActivated}/{totalPanelsToActivate})");

        UpdateVisual();

        if (panelsActivated >= totalPanelsToActivate)
        {
            DeactivateField();
        }
    }

    private void DeactivateField()
    {
        fieldActive = false;

        Debug.Log("=== ¡CAMPO MAGNÉTICO DESACTIVADO! ===");
        Debug.Log("Acércate al núcleo para desactivar el Ancla Madre");

        // Cambiar visual
        if (coreRenderer != null && damagedMaterial != null)
        {
            coreRenderer.material = damagedMaterial;
        }
    }

    private void UpdateVisual()
    {
        float damagePercent = (float)panelsActivated / totalPanelsToActivate;

        if (coreRenderer != null && normalMaterial != null)
        {
            Color currentColor = Color.Lerp(normalMaterial.color, Color.red, damagePercent);
            coreRenderer.material.color = currentColor;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isBossDefeated) return;

        if (other.CompareTag("Player") && !fieldActive)
        {
            DefeatBoss();
        }
        else if (other.CompareTag("Player") && fieldActive)
        {
            Debug.Log("¡El campo magnético te impide acercarte! Desactiva todos los paneles primero");
        }
    }

    private void DefeatBoss()
    {
        isBossDefeated = true;

        Debug.Log("=================================");
        Debug.Log("=== ¡ANCLA MADRE DESACTIVADA! ===");
        Debug.Log("=== ¡VICTORIA! ===");
        Debug.Log("=================================");

        // Desactivar el núcleo visualmente
        if (coreVisual != null)
        {
            coreVisual.SetActive(false);
        }

        // Esperar y cargar siguiente escena
        Invoke("LoadVictoryScene", 5f);
    }

    private void LoadVictoryScene()
    {
        Debug.Log($"Cargando: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    // Visualización en editor
    private void OnDrawGizmosSelected()
    {
        // Campo magnético
        Gizmos.color = fieldActive ? new Color(1f, 0f, 0f, 0.3f) : new Color(0.5f, 0.5f, 0.5f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, fieldRange);

        // Paneles
        if (panelPositions != null && panelPositions.Length > 0)
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
    }
    private void ShootProjectile()
    {
        if (projectilePrefab == null || firePoints.Length < 2 || playerTransform == null) return;

        Transform firePoint = firePoints[currentFireIndex];
        currentFireIndex = (currentFireIndex + 1) % firePoints.Length; // Alternar entre 0 y 1

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (playerTransform.position - firePoint.position).normalized;
            rb.velocity = direction * projectileSpeed;
        }
        else
        {
            Debug.LogWarning("El proyectil no tiene Rigidbody asignado.");
        }

        Debug.Log($"¡Disparo desde punto {currentFireIndex}!");
    }


}