using UnityEngine;

/// <summary>
/// Pickup para partes del cuerpo de R.U.B.O.
/// Coloca este script en los objetos que el jugador debe recoger
/// </summary>
public class BodyPartPickup : MonoBehaviour
{
    [Header("Part Type")]
    [SerializeField] private PartType partType = PartType.Legs;

    [Header("Visual Effects")]
    [SerializeField] private bool rotateObject = true;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private bool floatEffect = true;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    [Header("Particle Effects (Optional)")]
    [SerializeField] private GameObject pickupParticles;

    private Vector3 startPosition;
    private AudioSource audioSource;

    public enum PartType
    {
        Torso,
        Legs,
        Arms
    }

    void Start()
    {
        startPosition = transform.position;

        // Configurar audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }

        // Verificar si ya se recogió esta parte
        if (GameManager.Instance != null)
        {
            bool alreadyCollected = false;

            switch (partType)
            {
                case PartType.Torso:
                    alreadyCollected = GameManager.Instance.hasTorso;
                    break;
                case PartType.Legs:
                    alreadyCollected = GameManager.Instance.hasLegs;
                    break;
                case PartType.Arms:
                    alreadyCollected = GameManager.Instance.hasArms;
                    break;
            }

            // Si ya se recogió, destruir el objeto
            if (alreadyCollected)
            {
                Debug.Log($"Parte {partType} ya fue recogida - Destruyendo pickup");
                Destroy(gameObject);
                return;
            }
        }
    }

    void Update()
    {
        // Rotación continua
        if (rotateObject)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        // Efecto de flotación
        if (floatEffect)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectPart(other.gameObject);
        }
    }

    private void CollectPart(GameObject player)
    {
        // Reproducir sonido
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        // Crear partículas
        if (pickupParticles != null)
        {
            Instantiate(pickupParticles, transform.position, Quaternion.identity);
        }

        // Obtener PlayerStateLoader y notificar
        PlayerStateLoader stateLoader = player.GetComponent<PlayerStateLoader>();
        if (stateLoader != null)
        {
            stateLoader.OnPartCollected(partType.ToString());
        }
        else
        {
            Debug.LogWarning("PlayerStateLoader no encontrado en el jugador!");

            // Fallback: aplicar directamente
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                switch (partType)
                {
                    case PartType.Torso:
                        controller.ConnectTorso();
                        if (GameManager.Instance != null) GameManager.Instance.UnlockTorso();
                        break;
                    case PartType.Legs:
                        controller.ConnectLegs();
                        if (GameManager.Instance != null) GameManager.Instance.UnlockLegs();
                        break;
                    case PartType.Arms:
                        controller.ConnectArms();
                        if (GameManager.Instance != null) GameManager.Instance.UnlockArms();
                        break;
                }
            }
        }

        // Mostrar mensaje
        Debug.Log($"¡{GetPartName()} RECOGIDO!");

        // Destruir el objeto (con delay si hay sonido)
        if (pickupSound != null && audioSource != null)
        {
            // Desactivar visual pero mantener audio
            GetComponent<Renderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
            Destroy(gameObject, pickupSound.length);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private string GetPartName()
    {
        switch (partType)
        {
            case PartType.Torso: return "TORSO";
            case PartType.Legs: return "LEGS";
            case PartType.Arms: return "ARMS";
            default: return "PARTE DESCONOCIDA";
        }
    }

    // Gizmos para visualización en editor
    private void OnDrawGizmos()
    {
        Color gizmoColor = Color.green;

        switch (partType)
        {
            case PartType.Torso:
                gizmoColor = new Color(0f, 0.8f, 1f, 0.5f); 
                break;
            case PartType.Legs:
                gizmoColor = new Color(0f, 1f, 0f, 0.5f); 
                break;
            case PartType.Arms:
                gizmoColor = new Color(1f, 0.8f, 0f, 0.5f); 
                break;
        }

        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, 0.5f);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 1f); // Radio de recolección
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"PICKUP: {partType}");
#endif
    }
}