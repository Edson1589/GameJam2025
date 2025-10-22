using UnityEngine;

public class MagnetPanel : MonoBehaviour, IJammable
{
    [Header("Connected Drones")]
    [SerializeField] private PullerDrone[] connectedDrones;

    [Header("Panel Settings")]
    [SerializeField] private bool startActive = true;
    [SerializeField] private float cooldownTime = 3f;

    [Header("Visual")]
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private Material jammedMaterial;
    [SerializeField] private GameObject interactionPrompt;

    private Renderer panelRenderer;
    private bool isActive;
    private bool canInteract = true;
    private float cooldownTimer = 0f;
    private bool playerNearby = false;

    // Jamming
    private bool isJammed = false;
    private float jamTimer = 0f;

    void Start()
    {
        panelRenderer = GetComponent<Renderer>();
        isActive = startActive;
        UpdateVisual();
        UpdateDrones();

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        Debug.Log($"MagnetPanel '{gameObject.name}' inicializado - Estado: {(isActive ? "ACTIVO" : "INACTIVO")}");
    }

    void Update()
    {
        // Jamming timer
        if (isJammed)
        {
            jamTimer -= Time.deltaTime;
            if (jamTimer <= 0)
            {
                isJammed = false;
                UpdateVisual();
                Debug.Log($"Panel '{gameObject.name}' recuperado del jamming");
            }
        }

        // Cooldown (solo si no está jammeado)
        if (!canInteract && !isJammed)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                canInteract = true;
            }
        }

        // Interacción con E (solo si no está jammeado)
        if (playerNearby && canInteract && !isJammed && Input.GetKeyDown(KeyCode.E))
        {
            TogglePanel();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            if (interactionPrompt != null && !isJammed)
            {
                interactionPrompt.SetActive(true);
            }
            Debug.Log($"Jugador cerca del panel - Presiona E para {(isActive ? "DESACTIVAR" : "ACTIVAR")} imanes");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void TogglePanel()
    {
        isActive = !isActive;
        canInteract = false;
        cooldownTimer = cooldownTime;

        UpdateVisual();
        UpdateDrones();

        Debug.Log($"Panel '{gameObject.name}' cambiado a: {(isActive ? "ACTIVO" : "INACTIVO")}");
    }

    private void UpdateVisual()
    {
        if (panelRenderer != null)
        {
            if (isJammed)
                panelRenderer.material = jammedMaterial;
            else
                panelRenderer.material = isActive ? activeMaterial : inactiveMaterial;
        }
    }

    private void UpdateDrones()
    {
        if (connectedDrones == null || connectedDrones.Length == 0)
        {
            Debug.LogWarning($"MagnetPanel '{gameObject.name}' no tiene drones conectados!");
            return;
        }

        foreach (PullerDrone drone in connectedDrones)
        {
            if (drone != null)
            {
                if (isActive)
                {
                    drone.ActivateMagnet();
                }
                else
                {
                    drone.DeactivateMagnet();
                }
            }
        }
    }

    // Implementación de IJammable
    public void ApplyJam(float duration)
    {
        isJammed = true;
        jamTimer = duration;
        canInteract = false; // Desactiva la interacción mientras está jammeado

        // Oculta el prompt de interacción si está visible
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        UpdateVisual();
        Debug.Log($"Panel '{gameObject.name}' jammeado por {duration} segundos");
    }

    public bool IsJammed()
    {
        return isJammed;
    }
}