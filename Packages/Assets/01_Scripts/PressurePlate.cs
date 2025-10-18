using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;

    [Header("Connected Door")]
    [SerializeField] private TimedDoor connectedDoor;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private Renderer plateRenderer;
    private int objectsOnPlate = 0;
    private bool isActivated = false;

    void Start()
    {
        plateRenderer = GetComponent<Renderer>();
        UpdateVisual();

        if (showDebugInfo)
        {
            Debug.Log($"PressurePlate '{gameObject.name}' inicializada");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Solo cajas con tag "Pushable" activan la placa
        if (other.CompareTag("Pushable"))
        {
            objectsOnPlate++;

            if (showDebugInfo)
            {
                Debug.Log($"Objeto '{other.name}' entró en placa. Total: {objectsOnPlate}");
            }

            if (!isActivated && objectsOnPlate > 0)
            {
                Activate();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pushable"))
        {
            objectsOnPlate--;

            if (showDebugInfo)
            {
                Debug.Log($"Objeto '{other.name}' salió de placa. Total: {objectsOnPlate}");
            }

            if (isActivated && objectsOnPlate <= 0)
            {
                Deactivate();
            }
        }
    }

    private void Activate()
    {
        isActivated = true;
        UpdateVisual();

        if (connectedDoor != null)
        {
            connectedDoor.Open();
        }
        else
        {
            Debug.LogWarning("PressurePlate: No hay puerta conectada!");
        }

        Debug.Log("? Placa de presión ACTIVADA");
    }

    private void Deactivate()
    {
        isActivated = false;
        UpdateVisual();

        if (connectedDoor != null)
        {
            connectedDoor.Close();
        }

        Debug.Log("? Placa de presión DESACTIVADA");
    }

    private void UpdateVisual()
    {
        if (plateRenderer != null)
        {
            if (isActivated && activeMaterial != null)
            {
                plateRenderer.material = activeMaterial;
            }
            else if (!isActivated && inactiveMaterial != null)
            {
                plateRenderer.material = inactiveMaterial;
            }
        }
    }
}