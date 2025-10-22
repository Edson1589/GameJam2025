using UnityEngine;

public class ArmsPickup : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private float rotationSpeed = 80f;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatSpeed = 2.5f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
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

        if (player != null)
        {
            // Conectar los brazos
            player.ConnectArms();

            // Destruir el pickup
            Destroy(gameObject);
        }
    }
}