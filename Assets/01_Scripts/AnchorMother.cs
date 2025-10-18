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
        Debug.Log($"Activa {totalPanelsToActivate} paneles para debilitar el campo magn�tico");
    }

    void Update()
    {
        if (isBossDefeated) return;

        // Rotar el n�cleo
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
    }

    void FixedUpdate()
    {
        if (isBossDefeated || !fieldActive) return;

        // Aplicar campo magn�tico al jugador
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

        Debug.Log("=== �CAMPO MAGN�TICO DESACTIVADO! ===");
        Debug.Log("Ac�rcate al n�cleo para desactivar el Ancla Madre");

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
            Debug.Log("�El campo magn�tico te impide acercarte! Desactiva todos los paneles primero");
        }
    }

    private void DefeatBoss()
    {
        isBossDefeated = true;

        Debug.Log("=================================");
        Debug.Log("=== �ANCLA MADRE DESACTIVADA! ===");
        Debug.Log("=== �VICTORIA! ===");
        Debug.Log("=================================");

        // Desactivar el n�cleo visualmente
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

    // Visualizaci�n en editor
    private void OnDrawGizmosSelected()
    {
        // Campo magn�tico
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
}