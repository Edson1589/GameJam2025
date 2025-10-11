using UnityEngine;

public class LegsPickup : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatSpeed = 2f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;

        // Asegurarse que el Capsule Collider sea trigger
        CapsuleCollider col = GetComponent<CapsuleCollider>();
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
        // Verificar si es el jugador
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            // Conectar las piernas
            player.ConnectLegs();

            // Destruir el pickup
            Destroy(gameObject);
        }
    }
}