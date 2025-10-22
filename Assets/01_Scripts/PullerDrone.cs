using UnityEngine;

public class PullerDrone : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float waypointReachDistance = 0.5f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float detectionAngle = 60f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Magnet Pull")]
    [SerializeField] private float pullForce = 10f;
    [SerializeField] private float pullRange = 10f;
    [SerializeField] private bool magnetActive = true;

    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.blue;
    [SerializeField] private Color alertColor = Color.red;
    [SerializeField] private GameObject visionCone;

    private int currentPatrolIndex = 0;
    private bool playerDetected = false;
    private Transform detectedPlayer;
    private Renderer droneRenderer;
    private Material droneMaterial;

    void Start()
    {
        droneRenderer = GetComponent<Renderer>();
        if (droneRenderer != null)
        {
            droneMaterial = droneRenderer.material;
        }

        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning($"PullerDrone '{gameObject.name}' no tiene puntos de patrulla asignados!");
        }

        UpdateVisualState();
    }

    void Update()
    {
        if (!magnetActive)
        {
            // Si el im�n esta desactivado, solo patrullar
            Patrol();
            playerDetected = false;
            detectedPlayer = null;
            UpdateVisualState();
            return;
        }

        // Detectar jugador
        DetectPlayer();

        if (playerDetected && detectedPlayer != null)
        {
            // Si detect� al jugador, mirar hacia �l
            LookAtPlayer();
        }
        else
        {
            // Si no hay jugador detectado, patrullar
            Patrol();
        }

        UpdateVisualState();
    }

    void FixedUpdate()
    {
        // Aplicar fuerza de atracci�n si detecto al jugador
        if (magnetActive && playerDetected && detectedPlayer != null)
        {
            PullPlayer();
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];

        // Moverse hacia el punto de patrulla
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Rotar hacia el punto de patrulla
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Verificar si lleg� al punto
        if (Vector3.Distance(transform.position, targetPoint.position) < waypointReachDistance)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void DetectPlayer()
    {
        // Buscar al jugador en el rango
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);

        playerDetected = false;
        detectedPlayer = null;

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                Vector3 directionToPlayer = (col.transform.position - transform.position).normalized;
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

                // Verificar si est� dentro del cono de visi�n
                if (angleToPlayer < detectionAngle / 2f)
                {
                    // Raycast para verificar que no hay obst�culos
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange, playerLayer | obstacleLayer))
                    {
                        if (hit.collider.CompareTag("Player"))
                        {
                            playerDetected = true;
                            detectedPlayer = col.transform;
                            Debug.Log($"Dron '{gameObject.name}' detect� al jugador!");
                            break;
                        }
                    }
                }
            }
        }
    }

    private void LookAtPlayer()
    {
        if (detectedPlayer == null) return;

        Vector3 direction = (detectedPlayer.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * 2f * Time.deltaTime);
    }

    private void PullPlayer()
    {
        if (detectedPlayer == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, detectedPlayer.position);

        if (distanceToPlayer <= pullRange)
        {
            Rigidbody playerRb = detectedPlayer.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                Vector3 pullDirection = (transform.position - detectedPlayer.position).normalized;
                playerRb.AddForce(pullDirection * pullForce, ForceMode.Force);
            }
        }
    }

    private void UpdateVisualState()
    {
        if (droneMaterial != null)
        {
            if (!magnetActive)
            {
                droneMaterial.color = Color.gray;
            }
            else if (playerDetected)
            {
                droneMaterial.color = alertColor;
            }
            else
            {
                droneMaterial.color = normalColor;
            }
        }
    }

    public void ActivateMagnet()
    {
        magnetActive = true;
        Debug.Log($"Dron '{gameObject.name}' - Im�n ACTIVADO");
    }

    public void DeactivateMagnet()
    {
        magnetActive = false;
        playerDetected = false;
        detectedPlayer = null;
        Debug.Log($"Dron '{gameObject.name}' - Im�n DESACTIVADO");
    }

    private void OnDrawGizmosSelected()
    {
        // Rango de detecci�n
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de atracci�n
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pullRange);

        // Cono de visi�n
        Gizmos.color = Color.cyan;
        Vector3 forward = transform.forward * detectionRange;
        Vector3 left = Quaternion.Euler(0, -detectionAngle / 2f, 0) * forward;
        Vector3 right = Quaternion.Euler(0, detectionAngle / 2f, 0) * forward;

        Gizmos.DrawLine(transform.position, transform.position + forward);
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);

        // Puntos de patrulla
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);

                    // L�nea al siguiente punto
                    int nextIndex = (i + 1) % patrolPoints.Length;
                    if (patrolPoints[nextIndex] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                    }
                }
            }
        }
    }
}