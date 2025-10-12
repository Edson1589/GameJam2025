using UnityEngine;

public class BeltSwitch : MonoBehaviour
{
    [Header("Connected Belt")]
    [SerializeField] private ConveyorBelt connectedBelt;

    [Header("Switch Type")]
    [SerializeField] private SwitchType type = SwitchType.Toggle;

    [Header("Visual")]
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;

    private Renderer switchRenderer;
    private bool isOn = false;
    private bool canActivate = true;
    private float cooldown = 0.5f;
    private float cooldownTimer = 0f;

    public enum SwitchType
    {
        Toggle,          // Alterna entre on/off
        ReverseDirection, // Invierte dirección de cinta
        Activate,        // Solo activa
        Deactivate       // Solo desactiva
    }

    void Start()
    {
        switchRenderer = GetComponent<Renderer>();
        UpdateVisual();
    }

    void Update()
    {
        // Cooldown
        if (!canActivate)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                canActivate = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Solo el jugador puede activar interruptores
        if (other.CompareTag("Player") && canActivate)
        {
            ActivateSwitch();
        }
    }

    private void ActivateSwitch()
    {
        if (connectedBelt == null)
        {
            Debug.LogWarning($"Switch '{gameObject.name}' no tiene cinta conectada!");
            return;
        }

        canActivate = false;
        cooldownTimer = cooldown;

        switch (type)
        {
            case SwitchType.Toggle:
                isOn = !isOn;
                if (isOn)
                    connectedBelt.Activate();
                else
                    connectedBelt.Deactivate();
                break;

            case SwitchType.ReverseDirection:
                connectedBelt.ReverseDirection();
                isOn = !isOn;
                break;

            case SwitchType.Activate:
                connectedBelt.Activate();
                isOn = true;
                break;

            case SwitchType.Deactivate:
                connectedBelt.Deactivate();
                isOn = false;
                break;
        }

        UpdateVisual();
        Debug.Log($"Switch '{gameObject.name}' activado - Tipo: {type}");
    }

    private void UpdateVisual()
    {
        if (switchRenderer != null)
        {
            switchRenderer.material = isOn ? onMaterial : offMaterial;
        }
    }
}