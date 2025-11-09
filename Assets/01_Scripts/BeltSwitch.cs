using UnityEngine;

public class BeltSwitch : MonoBehaviour, IJammable
{
    [Header("Connected Belt")]
    [SerializeField] private ConveyorBelt connectedBelt;

    [Header("Switch Type")]
    [SerializeField] private SwitchType type = SwitchType.Toggle;

    [Header("Visual")]
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;
    [SerializeField] private Material jammedMaterial;

    private Renderer switchRenderer;
    private bool isOn = false;
    public bool canActivate = true;
    private float cooldown = 0.5f;
    private float cooldownTimer = 0f;

    private bool isJammed = false;
    private float jamTimer = 0f;

    public enum SwitchType
    {
        Toggle,          // Alterna entre on/off
        ReverseDirection, // Invierte direcci�n de cinta
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

        if (isJammed)
        {
            jamTimer -= Time.deltaTime;
            if (jamTimer <= 0)
            {
                isJammed = false;
                UpdateVisual();
                Debug.Log($"Switch '{gameObject.name}' recuperado del jamming");
            }
        }

        // Cooldown (solo si no esta jammeado)
        if (!canActivate && !isJammed)
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
        // Solo el jugador puede activar interruptores (solo si no esta jammeado)
        if (other.CompareTag("Player") && canActivate && !isJammed)
        {
            ActivateSwitch();
        }
    }

    public void ActivateSwitch()
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
            if (isJammed)
                switchRenderer.material = jammedMaterial;
            else
                switchRenderer.material = isOn ? onMaterial : offMaterial;
        }
    }

    // Implementaci�n de IJammable
    public void ApplyJam(float duration)
    {
        isJammed = true;
        jamTimer = duration;
        canActivate = false; // Desactiva la interacci�n mientras esto jammeado
        UpdateVisual();
        Debug.Log($"Switch '{gameObject.name}' jammeado por {duration} segundos");
    }

    public bool IsJammed()
    {
        return isJammed;
    }
}