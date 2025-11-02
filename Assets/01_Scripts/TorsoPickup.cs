using UnityEngine;

public class TorsoPickup : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private float rotationSpeed = 40f;
    [SerializeField] private float floatAmplitude = 0.4f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float pulseSpeed = 3f;

    [Header("Pickup Effects")]
    [SerializeField] private GameObject pickupParticles;
    [SerializeField] private AudioClip pickupSound;

    private Vector3 startPosition;
    private Renderer torsoRenderer;
    private Material torsoMaterial;
    private Color baseColor;
    private AudioSource audioSource;

    void Start()
    {
        startPosition = transform.position;
        torsoRenderer = GetComponent<Renderer>();

        if (torsoRenderer != null)
        {
            torsoMaterial = torsoRenderer.material;
            baseColor = torsoMaterial.color;
        }

        audioSource = GetComponent<AudioSource>();

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Update()
    {
        // Rotar lento
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        transform.Rotate(Vector3.forward, rotationSpeed * 0.3f * Time.deltaTime);

        // Flotar
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Pulso de brillo
        if (torsoMaterial != null)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float intensity = 1f + pulse * 0.5f;
            torsoMaterial.color = baseColor * intensity;

            if (torsoMaterial.IsKeywordEnabled("_EMISSION"))
            {
                torsoMaterial.SetColor("_EmissionColor", baseColor * pulse * 2f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && !player.hasTorso)
        {
            // Efectos visuales/audio
            if (pickupParticles != null)
            {
                Instantiate(pickupParticles, transform.position, Quaternion.identity);
            }

            if (pickupSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }

            // Conectar torso
            player.ConnectTorso();

            Debug.Log("¡TORSO CONECTADO! Ahora busca las PIERNAS");

            // Destruir el pickup
            Destroy(gameObject);
        }
    }
}