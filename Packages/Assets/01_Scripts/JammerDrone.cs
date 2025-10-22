using UnityEngine;

public class JammerDrone : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float waypointReachDistance = 0.5f;

    [Header("Jamming Settings")]
    [SerializeField] private float pulseInterval = 6f;
    [SerializeField] private float pulseRange = 8f;
    [SerializeField] private float jamDuration = 3f;
    [SerializeField] private LayerMask jammableLayer;

    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.cyan;
    [SerializeField] private Color pulseColor = Color.magenta;
    [SerializeField] private GameObject pulseEffect;

    [Header("Audio")]
    [SerializeField] private bool playSound = false;

    private int currentPatrolIndex = 0;
    private float pulseTimer = 0f;
    private bool isPulsing = false;
    private float pulseEffectTimer = 0f;
    private Renderer droneRenderer;
    private Material droneMaterial;
    private Color baseColor;

    void Start()
    {
        droneRenderer = GetComponent<Renderer>();
        if (droneRenderer != null)
        {
            droneMaterial = droneRenderer.material;
            baseColor = normalColor;
        }

        pulseTimer = pulseInterval;

        if (pulseEffect != null)
        {
            pulseEffect.SetActive(false);
        }

        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning($"JammerDrone '{gameObject.name}' no tiene puntos de patrulla!");
        }

        Debug.Log($"JammerDrone '{gameObject.name}' inicializado - Pulso cada {pulseInterval}s");
    }

    void Update()
    {
        // Patrullar
        Patrol();

        // Contador de pulsos
        pulseTimer -= Time.deltaTime;

        if (pulseTimer <= 0f && !isPulsing)
        {
            EmitPulse();
            pulseTimer = pulseInterval;
        }

        // Efecto visual del pulso
        if (isPulsing)
        {
            pulseEffectTimer -= Time.deltaTime;

            if (pulseEffectTimer <= 0f)
            {
                isPulsing = false;
                UpdateVisual(false);
            }
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];

        // Moverse
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Rotar
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Siguiente waypoint
        if (Vector3.Distance(transform.position, targetPoint.position) < waypointReachDistance)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void EmitPulse()
    {
        isPulsing = true;
        pulseEffectTimer = 0.5f; // Duración del efecto visual

        Debug.Log($">>> Dron '{gameObject.name}' emitió PULSO electromagnético!");

        UpdateVisual(true);

        // Activar efecto de partículas
        if (pulseEffect != null)
        {
            pulseEffect.SetActive(true);
            Invoke("DeactivatePulseEffect", 0.5f);
        }

        // Buscar objetos jammables en el rango
        Collider[] colliders = Physics.OverlapSphere(transform.position, pulseRange, jammableLayer);

        int jammedCount = 0;

        foreach (Collider col in colliders)
        {
            // Buscar componentes jammables
            IJammable jammable = col.GetComponent<IJammable>();

            if (jammable != null)
            {
                jammable.ApplyJam(jamDuration);
                jammedCount++;
            }

            // También buscar scripts específicos
            PressurePlate plate = col.GetComponent<PressurePlate>();
            if (plate != null)
            {
                // La placa será jammeada si implementa IJammable
            }

            BeltSwitch beltSwitch = col.GetComponent<BeltSwitch>();
            if (beltSwitch != null)
            {
                // El switch será jammeado si implementa IJammable
            }

            MagnetPanel magnetPanel = col.GetComponent<MagnetPanel>();
            if (magnetPanel != null)
            {
                // El panel será jammeado si implementa IJammable
            }
        }

        Debug.Log($"Pulse afectó {jammedCount} objetos");
    }

    private void DeactivatePulseEffect()
    {
        if (pulseEffect != null)
        {
            pulseEffect.SetActive(false);
        }
    }

    private void UpdateVisual(bool pulsing)
    {
        if (droneMaterial != null)
        {
            if (pulsing)
            {
                droneMaterial.color = pulseColor;
            }
            else
            {
                droneMaterial.color = normalColor;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Rango de pulso
        Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, pulseRange);

        // Puntos de patrulla
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);

                    int nextIndex = (i + 1) % patrolPoints.Length;
                    if (patrolPoints[nextIndex] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                    }
                }
            }
        }

        // Indicador de próximo pulso 
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Vector3 offset = Vector3.up * 2f;
            Gizmos.DrawLine(transform.position + offset, transform.position + offset + Vector3.up * (pulseTimer / pulseInterval) * 2f);
        }
    }
}