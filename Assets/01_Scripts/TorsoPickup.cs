using UnityEngine;

public class TorsoPickup : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private float rotationSpeed = 40f;
    [SerializeField] private float floatAmplitude = 0.4f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float pulseSpeed = 3f;

    private Vector3 startPosition;
    private Renderer torsoRenderer;
    private Material torsoMaterial;
    private Color baseColor;

    void Start()
    {
        startPosition = transform.position;

        torsoRenderer = GetComponent<Renderer>();
        if (torsoRenderer != null)
        {
            torsoMaterial = torsoRenderer.material;
            baseColor = torsoMaterial.color;
        }
    }

    void Update()
    {
        // Rotar más lento y majestuoso (es la pieza final)
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        transform.Rotate(Vector3.forward, rotationSpeed * 0.3f * Time.deltaTime);

        // Flotar
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Pulso de brillo intenso
        if (torsoMaterial != null)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float intensity = 1f + pulse * 0.5f;
            torsoMaterial.color = baseColor * intensity;

            // Emission también pulsa
            if (torsoMaterial.IsKeywordEnabled("_EMISSION"))
            {
                torsoMaterial.SetColor("_EmissionColor", baseColor * pulse * 2f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            player.ConnectTorso();
            Destroy(gameObject);
        }
    }
}