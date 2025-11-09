using UnityEngine;

public class EcoMemory : MonoBehaviour
{
    [Header("Memory Data")]
    [SerializeField] private int memoryID = 1;
    [SerializeField][TextArea(3, 6)] private string memoryText = "Eco-Memoria sin configurar";

    [Header("Visual Feedback")]
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float floatSpeed = 3f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.3f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField, Range(0f, 1f)] private float pickupVolume = 1f;

    private Vector3 startPosition;
    private Renderer memoryRenderer;
    private Material memoryMaterial;
    private Color baseColor;

    void Start()
    {
        startPosition = transform.position;

        memoryRenderer = GetComponent<Renderer>();
        if (memoryRenderer != null)
        {
            memoryMaterial = memoryRenderer.material;
            baseColor = memoryMaterial.color;
        }
    }

    void Update()
    {
        // Rotar suavemente
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Flotar arriba y abajo
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Pulso de brillo
        if (memoryMaterial != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            Color newColor = baseColor * (1f + pulse);
            memoryMaterial.color = newColor;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            CollectMemory(player);
        }
    }

    private void CollectMemory(PlayerController player)
    {
        if (pickupSfx)
            AudioSource.PlayClipAtPoint(pickupSfx, transform.position, pickupVolume);
        // Registrar en el gestor de coleccionables (si existe)
        MemoryManager manager = FindObjectOfType<MemoryManager>();
        if (manager != null)
        {
            manager.CollectMemory(memoryID, memoryText);
        }
        else
        {
            // Si no hay manager, mostrar directamente
            Debug.Log($"=== ECO-MEMORIA #{memoryID} ===");
            Debug.Log(memoryText);
            Debug.Log("========================");
        }

        // Destruir el objeto
        Destroy(gameObject);
    }

    // Visualizaci�n en editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}