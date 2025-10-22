using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [Header("Connected Door")]
    [SerializeField] private SlidingDoor connectedDoor;

    [Header("Button Type")]
    [SerializeField] private ButtonType type = ButtonType.PressurePlate;

    [Header("Visual")]
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private GameObject interactionPrompt;

    [Header("Animation")]
    [SerializeField] private float pressDepth = 0.2f;

    private Renderer buttonRenderer;
    private bool isActive = false;
    private bool playerNearby = false;
    private Vector3 originalPosition;
    private Vector3 pressedPosition;

    public enum ButtonType
    {
        PressurePlate,  // Se activa al pisar
        Lever           // Se activa con E
    }

    void Start()
    {
        buttonRenderer = GetComponent<Renderer>();
        originalPosition = transform.position;
        pressedPosition = originalPosition + Vector3.down * pressDepth;

        UpdateVisual();

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (connectedDoor == null)
        {
            Debug.LogWarning($"DoorButton '{gameObject.name}' no tiene puerta conectada!");
        }
    }

    void Update()
    {
        // Lever type - requiere E
        if (type == ButtonType.Lever && playerNearby && !isActive && Input.GetKeyDown(KeyCode.E))
        {
            Activate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (type == ButtonType.Lever)
            {
                playerNearby = true;

                if (interactionPrompt != null && !isActive)
                {
                    interactionPrompt.SetActive(true);
                }

                if (!isActive)
                {
                    Debug.Log("Presiona E para activar la palanca");
                }
            }
            else if (type == ButtonType.PressurePlate)
            {
                // Pressure plate se activa automáticamente
                if (!isActive)
                {
                    Activate();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (type == ButtonType.Lever)
            {
                playerNearby = false;

                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(false);
                }
            }
        }
    }

    private void Activate()
    {
        if (connectedDoor == null) return;

        isActive = true;
        UpdateVisual();
        AnimatePress();

        // Abrir puerta
        connectedDoor.Open();

        Debug.Log($"Botón '{gameObject.name}' ACTIVADO - Puerta abierta por 5s");

        // Auto-desactivar después de que la puerta se cierre
        Invoke("Deactivate", connectedDoor != null ? 5.5f : 5f);
    }

    private void Deactivate()
    {
        isActive = false;
        UpdateVisual();
        AnimateRelease();

        Debug.Log($"Botón '{gameObject.name}' DESACTIVADO");
    }

    private void AnimatePress()
    {
        // Animar el botón hundiéndose
        transform.position = pressedPosition;
    }

    private void AnimateRelease()
    {
        // Animar el botón subiendo
        transform.position = originalPosition;
    }

    private void UpdateVisual()
    {
        if (buttonRenderer != null)
        {
            if (isActive && activeMaterial != null)
            {
                buttonRenderer.material = activeMaterial;
            }
            else if (!isActive && inactiveMaterial != null)
            {
                buttonRenderer.material = inactiveMaterial;
            }
        }
    }
}