using UnityEngine;

/// <summary>
/// Activa el portal solo cuando la puerta se abre completamente
/// </summary>
public class PortalDoorActivator : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SlidingDoor door;
    [SerializeField] private LevelPortal portal;

    [Header("Settings")]
    [SerializeField] private bool activateOnDoorOpen = true;
    [SerializeField] private float checkInterval = 0.5f; // Verificar cada medio segundo

    private bool portalWasActivated = false;
    private float checkTimer = 0f;

    void Start()
    {
        // Validar referencias
        if (door == null)
        {
            Debug.LogError("¡No se asignó la puerta en PortalDoorActivator!");
            enabled = false;
            return;
        }

        if (portal == null)
        {
            Debug.LogError("¡No se asignó el portal en PortalDoorActivator!");
            enabled = false;
            return;
        }

        // Desactivar portal al inicio
        if (activateOnDoorOpen)
        {
            portal.DeactivatePortal();
            Debug.Log("Portal desactivado - Se activará cuando la puerta se abra");
        }
    }

    void Update()
    {
        if (!activateOnDoorOpen) return;

        // Verificar periódicamente el estado de la puerta
        checkTimer += Time.deltaTime;

        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;

            // Sincronizar el estado del portal con el de la puerta
            if (door.IsOpen() && !portalWasActivated)
            {
                ActivatePortal();
            }
            else if (!door.IsOpen() && portalWasActivated)
            {
                DeactivatePortal();
            }
        }
    }

    private void ActivatePortal()
    {
        portalWasActivated = true;
        portal.ActivatePortal();
        Debug.Log("✓ ¡Puerta abierta! Portal activado");
    }

    private void DeactivatePortal()
    {
        portalWasActivated = false;
        portal.DeactivatePortal();
        Debug.Log("✗ ¡Puerta cerrada! Portal desactivado");
    }

    // Método público por si quieres activarlo manualmente
    public void ForceActivatePortal()
    {
        ActivatePortal();
    }
}