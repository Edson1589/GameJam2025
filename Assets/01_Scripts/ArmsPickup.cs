using UnityEngine;

public class ArmsPickup : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private float rotationSpeed = 80f;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatSpeed = 2.5f;

    [Header("Pickup Effects")]
    [SerializeField] private GameObject pickupParticles;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject completionEffect; 

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
        // Rotar en múltiples ejes
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        transform.Rotate(Vector3.right, rotationSpeed * 0.5f * Time.deltaTime);

        // Flotar arriba y abajo
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && !player.hasArms)
        {
            // Verificar que tenga torso y piernas primero
            if (!player.hasTorso || !player.hasLegs)
            {
                Debug.Log("¡Necesitas el TORSO y las PIERNAS primero!");
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

            // Conectar brazos 
            player.ConnectArms();

            // Efecto especial de completado
            if (player.IsFullyAssembled() && completionEffect != null)
            {
                Instantiate(completionEffect, player.transform.position, Quaternion.identity);
                Debug.Log("🤖 ¡ENSAMBLAJE COMPLETO! R.U.B.O. está FULLY OPERATIONAL");
            }

            Debug.Log("¡BRAZOS CONECTADOS! Ahora puedes empujar cajas y usar palancas");

            // Destruir el pickup
            Destroy(gameObject);
        }
    }
}