using UnityEngine;

public class LegsPickup : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("Pickup Effects")]
    [SerializeField] private GameObject pickupParticles;
    [SerializeField] private AudioClip pickupSound;

    private Vector3 startPosition;
    private AudioSource audioSource;

    void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();

        // Asegurar que el collider sea trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Update()
    {
        // Rotar constantemente
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Flotar arriba y abajo
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && !player.hasLegs)
        {
            // Verificar que tenga el torso primero 
            if (!player.hasTorso)
            {
                Debug.Log("¡Necesitas el TORSO primero!");
                return;
            }

            // Efectos visuales/audio
            if (pickupParticles != null)
            {
                Instantiate(pickupParticles, transform.position, Quaternion.identity);
            }

            if (pickupSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }

            // Conectar piernas 
            player.ConnectLegs();

            Debug.Log("¡PIERNAS CONECTADAS! Salto y Dash desbloqueados. Busca los BRAZOS");

            // Destruir el pickup
            Destroy(gameObject);
        }
    }
}