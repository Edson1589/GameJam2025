using UnityEngine;

/// <summary>
/// Animación simple de cámara - Panorámica del nivel
/// </summary>
public class CameraIntroAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Transform[] cameraPoints;
    [SerializeField] private float animationDuration = 6f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Skip Settings")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private bool canSkip = true;

    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private Camera mainCamera;

    [Header("Original Camera Settings")]
    [SerializeField] private Vector3 finalCameraOffset = new Vector3(0, 8, -10);

    private float animationTimer = 0f;
    private bool isAnimating = true;

    private ThirdPersonCamera thirdPersonCam;
    private CameraFollow cameraFollow;

    void Start()
    {
        // Desactivar control del jugador
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = false;
                Debug.Log("✓ Control del jugador desactivado para intro");
            }
        }

        if (mainCamera != null)
        {
            thirdPersonCam = mainCamera.GetComponent<ThirdPersonCamera>();
            cameraFollow = mainCamera.GetComponent<CameraFollow>();

            if (thirdPersonCam != null)
            {
                thirdPersonCam.enabled = false;
                Debug.Log("✓ ThirdPersonCamera desactivado temporalmente");
            }

            if (cameraFollow != null)
            {
                cameraFollow.enabled = false;
                Debug.Log("✓ CameraFollow desactivado temporalmente");
            }
        }

        // Verificar que tengamos puntos
        if (cameraPoints == null || cameraPoints.Length < 2)
        {
            Debug.LogError("¡Necesitas al menos 2 Camera Points!");
            EndAnimation();
        }
    }

    void Update()
    {
        if (!isAnimating) return;

        // Permitir saltar la intro
        if (canSkip && Input.GetKeyDown(skipKey))
        {
            Debug.Log("⏭️ Intro saltada por el jugador");
            EndAnimation();
            return;
        }

        animationTimer += Time.deltaTime;
        float progress = animationTimer / animationDuration;

        if (progress >= 1f)
        {
            EndAnimation();
            return;
        }

        AnimateCamera(progress);
    }

    void AnimateCamera(float progress)
    {
        if (cameraPoints.Length < 2) return;

        float easedProgress = easeCurve.Evaluate(progress);

        float totalSegments = cameraPoints.Length - 1;
        float currentSegment = easedProgress * totalSegments;
        int segmentIndex = Mathf.FloorToInt(currentSegment);
        float segmentProgress = currentSegment - segmentIndex;

        segmentIndex = Mathf.Min(segmentIndex, cameraPoints.Length - 2);

        // Interpolar posición y rotación
        Transform pointA = cameraPoints[segmentIndex];
        Transform pointB = cameraPoints[segmentIndex + 1];

        mainCamera.transform.position = Vector3.Lerp(
            pointA.position,
            pointB.position,
            segmentProgress
        );

        mainCamera.transform.rotation = Quaternion.Slerp(
            pointA.rotation,
            pointB.rotation,
            segmentProgress
        );
    }

    void EndAnimation()
    {
        isAnimating = false;

        if (thirdPersonCam != null)
        {
            thirdPersonCam.enabled = true;
            Debug.Log("✓ ThirdPersonCamera reactivado");
        }

        if (cameraFollow != null)
        {
            cameraFollow.enabled = true;
            Debug.Log("✓ CameraFollow reactivado");
        }

        // Reactivar jugador
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = true;
                Debug.Log("✓ Control del jugador reactivado");
            }
        }

        Debug.Log("🎬 Intro terminada - Control devuelto al jugador");

        Destroy(gameObject, 0.5f);
    }

    void OnDrawGizmos()
    {
        if (cameraPoints == null || cameraPoints.Length < 2) return;

        // Dibujar el path de la cámara
        Gizmos.color = Color.cyan;
        for (int i = 0; i < cameraPoints.Length - 1; i++)
        {
            if (cameraPoints[i] != null && cameraPoints[i + 1] != null)
            {
                Gizmos.DrawLine(cameraPoints[i].position, cameraPoints[i + 1].position);

                // Dibuja una esfera en cada punto
                Gizmos.DrawWireSphere(cameraPoints[i].position, 0.5f);

                // Dibuja la dirección de la cámara
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(cameraPoints[i].position, cameraPoints[i].forward * 2f);
                Gizmos.color = Color.cyan;
            }
        }

        // Último punto
        if (cameraPoints[cameraPoints.Length - 1] != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(cameraPoints[cameraPoints.Length - 1].position, 0.5f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(cameraPoints[cameraPoints.Length - 1].position,
                          cameraPoints[cameraPoints.Length - 1].forward * 2f);
        }

        // Dibujar números de los puntos
#if UNITY_EDITOR
        for (int i = 0; i < cameraPoints.Length; i++)
        {
            if (cameraPoints[i] != null)
            {
                UnityEditor.Handles.Label(
                    cameraPoints[i].position + Vector3.up * 0.5f,
                    $"Point {i + 1}"
                );
            }
        }
#endif
    }
}