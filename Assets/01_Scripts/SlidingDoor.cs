using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    [Header("Door Panels")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("Door Settings")]
    [SerializeField] private float openDistance = 3f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private float openDuration = 5f;

    [Header("Audio")]
    [SerializeField] private bool playSounds = false;

    private Vector3 leftClosedPosition;
    private Vector3 leftOpenPosition;
    private Vector3 rightClosedPosition;
    private Vector3 rightOpenPosition;

    private bool isOpen = false;
    private bool isMoving = false;
    private float openTimer = 0f;

    void Start()
    {
        // Guardar posiciones iniciales
        if (leftDoor != null)
        {
            leftClosedPosition = leftDoor.position;
            leftOpenPosition = leftClosedPosition + transform.forward * openDistance;
        }

        if (rightDoor != null)
        {
            rightClosedPosition = rightDoor.position;
            rightOpenPosition = rightClosedPosition + transform.forward * -openDistance;
        }

        Debug.Log($"SlidingDoor '{gameObject.name}' inicializada - Distancia: {openDistance}m");
    }

    void Update()
    {
        // Timer de auto-cierre
        if (isOpen && !isMoving)
        {
            openTimer -= Time.deltaTime;

            if (openTimer <= 0f)
            {
                Close();
            }
        }

        // Mover puertas suavemente
        if (isMoving)
        {
            bool leftReached = true;
            bool rightReached = true;

            if (leftDoor != null)
            {
                Vector3 leftTarget = isOpen ? leftOpenPosition : leftClosedPosition;
                leftDoor.position = Vector3.MoveTowards(leftDoor.position, leftTarget, openSpeed * Time.deltaTime);
                leftReached = Vector3.Distance(leftDoor.position, leftTarget) < 0.01f;
            }

            if (rightDoor != null)
            {
                Vector3 rightTarget = isOpen ? rightOpenPosition : rightClosedPosition;
                rightDoor.position = Vector3.MoveTowards(rightDoor.position, rightTarget, openSpeed * Time.deltaTime);
                rightReached = Vector3.Distance(rightDoor.position, rightTarget) < 0.01f;
            }

            // Verificar si ambas llegaron
            if (leftReached && rightReached)
            {
                isMoving = false;

                if (isOpen)
                {
                    Debug.Log($"Puerta '{gameObject.name}' ABIERTA - Se cerrará en {openDuration}s");
                }
                else
                {
                    Debug.Log($"Puerta '{gameObject.name}' CERRADA");
                }
            }
        }
    }

    public void Open()
    {
        if (!isOpen || isMoving)
        {
            isOpen = true;
            isMoving = true;
            openTimer = openDuration;
            Debug.Log($">>> Puerta '{gameObject.name}' ABRIENDO...");
        }
        else
        {
            // Si ya está abierta, reiniciar timer
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
            Debug.Log($"<<< Puerta '{gameObject.name}' CERRANDO...");
        }
    }

    public bool IsOpen()
    {
        return isOpen && !isMoving;
    }

    private void OnDrawGizmos()
    {
        // Actualizar posiciones si no está jugando
        if (!Application.isPlaying)
        {
            if (leftDoor != null)
            {
                leftClosedPosition = leftDoor.position;
                leftOpenPosition = leftClosedPosition + transform.forward * openDistance;
            }

            if (rightDoor != null)
            {
                rightClosedPosition = rightDoor.position;
                rightOpenPosition = rightClosedPosition + transform.forward * -openDistance;
            }
        }

        // Dibujar posiciones abiertas de la puerta IZQUIERDA
        if (leftDoor != null)
        {
            // Cubo verde en posición abierta
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Verde semitransparente
            Gizmos.DrawCube(leftOpenPosition, leftDoor.localScale);

            // Contorno verde
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(leftOpenPosition, leftDoor.localScale);

            // Línea amarilla de recorrido
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(leftClosedPosition, leftOpenPosition);
        }

        // Dibujar posiciones abiertas de la puerta DERECHA
        if (rightDoor != null)
        {
            // Cubo verde en posición abierta
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Verde semitransparente
            Gizmos.DrawCube(rightOpenPosition, rightDoor.localScale);

            // Contorno verde
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(rightOpenPosition, rightDoor.localScale);

            // Línea amarilla de recorrido
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(rightClosedPosition, rightOpenPosition);
        }

        // Dibujar el marco de la puerta (opcional)
        DrawDoorFrame();
    }

    private void DrawDoorFrame()
    {
        if (leftDoor == null || rightDoor == null) return;

        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Gris semitransparente

        // Obtener el punto medio entre las dos puertas
        Vector3 center = (leftClosedPosition + rightClosedPosition) / 2f;

        // Calcular el ancho total del marco
        float doorWidth = Vector3.Distance(leftClosedPosition, rightClosedPosition);
        float doorHeight = leftDoor.localScale.y;

        // Dibujar marco superior
        Vector3 topCenter = center + Vector3.up * (doorHeight / 2f + 0.15f);
        Gizmos.DrawWireCube(topCenter, new Vector3(0.3f, 0.3f, doorWidth + 0.6f));
    }
}