using UnityEngine;
using System.Collections;

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

    [Header("Ground Snap (opcional)")]
    [SerializeField] private float groundProbeUp = 2.0f;   // cuánto arriba lanzar el raycast
    [SerializeField] private float groundProbeDown = 5.0f; // cuánto abajo buscar suelo
    [SerializeField] private LayerMask groundMask = ~0;    // por defecto, todo

    private Vector3 startPosition;
    private Renderer torsoRenderer;
    private Material torsoMaterial;
    private Color baseColor;
    private AudioSource audioSource;
    [SerializeField] private PlayerAmmoSystem Ammo;

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

        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void Update()
    {
        // Rotación + flotación + pulso
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        transform.Rotate(Vector3.forward, rotationSpeed * 0.3f * Time.deltaTime);

        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        if (torsoMaterial != null)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float intensity = 1f + pulse * 0.5f;
            torsoMaterial.color = baseColor * intensity;
            if (torsoMaterial.IsKeywordEnabled("_EMISSION"))
                torsoMaterial.SetColor("_EmissionColor", baseColor * pulse * 2f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null && !player.hasTorso)
        {
            // Evitar dobles triggers mientras se procesa
            var myCol = GetComponent<Collider>();
            if (myCol) myCol.enabled = false;
            if (torsoRenderer) torsoRenderer.enabled = false;

            StartCoroutine(PickupRoutine(player));
        }
    }

    private IEnumerator PickupRoutine(PlayerController player)
    {
        // VFX/SFX desacoplados del objeto por si lo destruimos
        if (pickupParticles) Instantiate(pickupParticles, transform.position, Quaternion.identity);
        if (pickupSound) AudioSource.PlayClipAtPoint(pickupSound, transform.position, 1f);

        // 1) Cambios de gameplay
        player.ConnectTorso();
        if (Ammo) Ammo.AddAmmo(10);

        // 2) Garantizar colisión válida en el player
        var cc = player.GetComponent<CharacterController>();
        var cap = player.GetComponent<CapsuleCollider>();
        var rb = player.GetComponent<Rigidbody>();

        if (rb)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        if (cap) cap.isTrigger = false; // por si algún flujo lo dejó en trigger

        // *Muy importante:* esperar a FixedUpdate para que Física procese
        yield return new WaitForFixedUpdate();

        // 3) Re-anclar al suelo (evita quedar insertado y atravesar)
        float halfHeight = 0.9f; // fallback
        if (cc) halfHeight = cc.height * 0.5f + cc.skinWidth;
        else if (cap) halfHeight = cap.height * 0.5f + Mathf.Max(0.01f, cap.radius * 0.1f);

        Vector3 origin = player.transform.position + Vector3.up * groundProbeUp;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundProbeUp + groundProbeDown, groundMask, QueryTriggerInteraction.Ignore))
        {
            player.transform.position = hit.point + Vector3.up * (halfHeight + 0.01f);
        }

        // 4) Destruir el pickup
        Destroy(gameObject);
    }
}
