using UnityEngine;

public class TimedDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openHeight = 4f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private float openDuration = 5f; 

    [Header("Audio (Optional)")]
    [SerializeField] private bool playSound = false;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;
    private bool isMoving = false;
    private float openTimer = 0f;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * openHeight;

        Debug.Log($"Door '{gameObject.name}' inicializada en {closedPosition}");
    }

    void Update()
    {
        // Si está abierta, contar tiempo hacia el cierre
        if (isOpen && !isMoving)
        {
            openTimer -= Time.deltaTime;

            if (openTimer <= 0)
            {
                Close();
            }
        }

        // Mover la puerta suavemente
        if (isMoving)
        {
            Vector3 targetPosition = isOpen ? openPosition : closedPosition;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, openSpeed * Time.deltaTime);

            // Verificar si llegó al destino
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;

                if (isOpen)
                {
                    Debug.Log($"Puerta completamente ABIERTA - Se cerrará en {openDuration}s");
                }
                else
                {
                    Debug.Log("Puerta completamente CERRADA");
                }
            }
        }
    }

    public void Open()
    {
        if (!isOpen)
        {
            isOpen = true;
            isMoving = true;
            openTimer = openDuration;
            Debug.Log($">>> Puerta ABRIENDO - Durará {openDuration} segundos");
        }
        else
        {
            // Si ya estaba abierta, reiniciar el timer
            openTimer = openDuration;
            Debug.Log($">>> Timer de puerta REINICIADO ({openDuration}s)");
        }
    }

    public void Close()
    {
        if (isOpen)
        {
            isOpen = false;
            isMoving = true;
            Debug.Log("<<< Puerta CERRANDO");
        }
    }

    // Visualización en editor
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            closedPosition = transform.position;
            openPosition = closedPosition + Vector3.up * openHeight;
        }

        // Dibujar posición abierta
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(openPosition, transform.localScale);

        // Línea de conexión
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(closedPosition, openPosition);
    }
}