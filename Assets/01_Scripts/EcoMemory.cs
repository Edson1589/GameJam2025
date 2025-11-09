using UnityEngine;
using UnityEngine.Localization;
using System.Collections;

public class EcoMemory : MonoBehaviour
{
    [Header("Memory Data")]
    [SerializeField] private int memoryID = 1;
    [SerializeField] private LocalizedString localizedMemoryText;

    [Header("Visual Feedback")]
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float floatSpeed = 3f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.3f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSfx;

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
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

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
            StartCoroutine(CollectMemoryCoroutine(player));
        }
    }

    private IEnumerator CollectMemoryCoroutine(PlayerController player)
    {
        AudioSource playerSFXSource = player.GetComponent<AudioSource>();
        if (playerSFXSource != null && pickupSfx != null)
        {
            playerSFXSource.PlayOneShot(pickupSfx);
        }

        string memoryText = "";

        if (localizedMemoryText != null)
        {
            var op = localizedMemoryText.GetLocalizedStringAsync();
            yield return op;

            if (op.IsDone && op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                memoryText = op.Result;
            }
            else
            {
                Debug.LogError("Fallo al cargar la memoria localizada: " + localizedMemoryText.TableEntryReference.Key);
                memoryText = "ERROR DE CARGA: " + localizedMemoryText.TableEntryReference.Key;
            }
        }

        MemoryManager manager = FindObjectOfType<MemoryManager>();
        if (manager != null)
        {
            manager.CollectMemory(memoryID, memoryText);
        }
        else
        {
            Debug.Log("=== ECO-MEMORIA #" + memoryID + " ===");
            Debug.Log(memoryText);
            Debug.Log("========================");
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}