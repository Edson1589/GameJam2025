using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelPortal : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private string nextSceneName = "Level2";
    [Tooltip("O usa el índice de la escena en Build Settings")]
    [SerializeField] private int nextSceneIndex = -1; // -1 = usar nombre

    [Header("Portal Settings")]
    [SerializeField] private bool portalActive = true;
    [SerializeField] private float activationDelay = 0.5f; // Delay antes de poder usar el portal
    [SerializeField] private string playerTag = "Player";

    [Header("Visual Effects")]
    [SerializeField] private bool rotatePortal = true;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private Color portalColor = new Color(0f, 0.8f, 1f, 0.5f); // Cyan
    [SerializeField] private bool pulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip portalSound;
    [SerializeField] private AudioClip teleportSound;

    private bool isTransitioning = false;
    private float initialScale;
    private AudioSource audioSource;
    private Renderer portalRenderer;
    private Material portalMaterial;

    void Start()
    {
        initialScale = transform.localScale.x;

        // Si el portal está desactivado al inicio, hacerlo invisible
        if (!portalActive)
        {
            transform.localScale = Vector3.zero;
        }

        // Configurar audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (portalSound != null || teleportSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }

        // Reproducir sonido ambiental del portal solo si está activo
        if (portalSound != null && audioSource != null && portalActive)
        {
            audioSource.clip = portalSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Configurar material del portal
        portalRenderer = GetComponent<Renderer>();
        if (portalRenderer != null)
        {
            portalMaterial = portalRenderer.material;

            if (portalActive)
            {
                portalMaterial.color = portalColor;

                // Hacer que el material emita luz
                if (portalMaterial.HasProperty("_EmissionColor"))
                {
                    portalMaterial.EnableKeyword("_EMISSION");
                    portalMaterial.SetColor("_EmissionColor", portalColor * 2f);
                }
            }
            else
            {
                portalMaterial.color = Color.clear;
            }
        }

        Debug.Log($"Portal '{gameObject.name}' inicializado - Nivel destino: {GetDestinationName()}");
    }

    void Update()
    {
        if (!portalActive) return;

        // Rotación del portal
        if (rotatePortal)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        // Efecto de pulso
        if (pulseEffect)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.1f;
            transform.localScale = Vector3.one * initialScale * pulse;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si es el jugador y el portal está activo
        if (other.CompareTag(playerTag) && portalActive && !isTransitioning)
        {
            Debug.Log($"¡Jugador detectado en portal! Iniciando transición a {GetDestinationName()}");
            StartCoroutine(TeleportPlayer(other.gameObject));
        }
    }

    private IEnumerator TeleportPlayer(GameObject player)
    {
        isTransitioning = true;

        // Reproducir sonido de teletransporte
        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }

        // Efecto visual: expandir portal
        float expandDuration = 0.5f;
        float timer = 0f;
        float startScale = initialScale;
        float endScale = initialScale * 1.5f;

        while (timer < expandDuration)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(startScale, endScale, timer / expandDuration);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }

        // Pequeño delay antes de cambiar de escena
        yield return new WaitForSeconds(activationDelay);

        // Cargar siguiente nivel
        LoadNextLevel();
    }

    // Efecto visual al activarse
    private IEnumerator ActivationEffect()
    {
        float duration = 0.5f;
        float timer = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * initialScale;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, timer / duration);

            // Actualizar color del material
            if (portalMaterial != null)
            {
                Color currentColor = Color.Lerp(Color.clear, portalColor, timer / duration);
                portalMaterial.color = currentColor;

                if (portalMaterial.HasProperty("_EmissionColor"))
                {
                    portalMaterial.SetColor("_EmissionColor", currentColor * 2f);
                }
            }

            yield return null;
        }

        transform.localScale = endScale;
    }

    // Efecto visual al desactivarse
    private IEnumerator DeactivationEffect()
    {
        float duration = 0.3f;
        float timer = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, timer / duration);

            // Actualizar color del material
            if (portalMaterial != null)
            {
                Color currentColor = Color.Lerp(portalColor, Color.clear, timer / duration);
                portalMaterial.color = currentColor;

                if (portalMaterial.HasProperty("_EmissionColor"))
                {
                    portalMaterial.SetColor("_EmissionColor", currentColor * 2f);
                }
            }

            yield return null;
        }

        transform.localScale = endScale;
    }

    private void LoadNextLevel()
    {
        if (nextSceneIndex >= 0)
        {
            // Cargar por índice
            Debug.Log($"Cargando escena por índice: {nextSceneIndex}");
            SceneManager.LoadScene(nextSceneIndex);
        }
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            // Cargar por nombre
            Debug.Log($"Cargando escena por nombre: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("¡No se especificó ninguna escena destino!");
        }
    }

    public void ActivatePortal()
    {
        if (!portalActive)
        {
            portalActive = true;
            StartCoroutine(ActivationEffect());
            Debug.Log($"Portal '{gameObject.name}' ACTIVADO");
        }
    }

    public void DeactivatePortal()
    {
        if (portalActive)
        {
            portalActive = false;
            StartCoroutine(DeactivationEffect());
            Debug.Log($"Portal '{gameObject.name}' DESACTIVADO");
        }
    }

    private string GetDestinationName()
    {
        if (nextSceneIndex >= 0)
            return $"Escena #{nextSceneIndex}";
        else if (!string.IsNullOrEmpty(nextSceneName))
            return nextSceneName;
        else
            return "No configurado";
    }

    // ═══════════════════════════════════════════════════════
    // VISUALIZACIÓN CON GIZMOS
    // ═══════════════════════════════════════════════════════
    private void OnDrawGizmos()
    {
        // Color según estado del portal
        Gizmos.color = portalActive ? new Color(0f, 1f, 1f, 0.3f) : new Color(1f, 0f, 0f, 0.3f);

        // Dibujar esfera del portal
        Gizmos.DrawSphere(transform.position, transform.localScale.x / 2f);

        // Dibujar contorno
        Gizmos.color = portalActive ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(transform.position, transform.localScale.x / 2f);

        // Dibujar flecha indicando dirección
        Gizmos.color = Color.yellow;
        Vector3 arrowStart = transform.position;
        Vector3 arrowEnd = transform.position + transform.forward * 2f;
        Gizmos.DrawLine(arrowStart, arrowEnd);
        Gizmos.DrawLine(arrowEnd, arrowEnd - transform.forward * 0.5f + transform.right * 0.3f);
        Gizmos.DrawLine(arrowEnd, arrowEnd - transform.forward * 0.5f - transform.right * 0.3f);
    }

    private void OnDrawGizmosSelected()
    {
        // Mostrar información del destino
        Gizmos.color = Color.green;
        Vector3 labelPos = transform.position + Vector3.up * 2f;

#if UNITY_EDITOR
        UnityEditor.Handles.Label(labelPos, $"→ {GetDestinationName()}");
#endif
    }
}