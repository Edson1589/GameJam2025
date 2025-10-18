using UnityEngine;

public class PickupTorso : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("Pickup Requirements")]
    [SerializeField] private bool requireLegs = true;
    [SerializeField] private bool requireArms = true;

    private Vector3 startPosition;
    private Renderer pickupRenderer;
    private Color originalColor;
    private bool isLocked = true;

    void Start()
    {
        startPosition = transform.position;
        pickupRenderer = GetComponent<Renderer>();

        if (pickupRenderer != null)
        {
            originalColor = pickupRenderer.material.color;
        }
    }

    void Update()
    {
        // Rotacion constante
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Efecto de flotacion
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Efecto de brillo pulsante
        if (pickupRenderer != null)
        {
            float pulse = Mathf.PingPong(Time.time * 0.5f, 0.5f);
            Color emissionColor = originalColor * (1 + pulse);
            pickupRenderer.material.SetColor("_EmissionColor", emissionColor * 0.3f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                // Verificar requisitos
                if (requireLegs && !player.hasLegs)
                {
                    Debug.Log("TORSO BLOQUEADO - Necesitas encontrar las PIERNAS primero!");
                    ShowLockedMessage();
                    return;
                }

                if (requireArms && !player.hasArms)
                {
                    Debug.Log("TORSO BLOQUEADO - Necesitas encontrar los BRAZOS primero!");
                    ShowLockedMessage();
                    return;
                }

                // Todo OK - Conectar el torso
                player.ConnectTorso();

                Debug.Log("========================================");
                Debug.Log("        TORSO RECONECTADO");
                Debug.Log("========================================");
                Debug.Log("+ Sistema de iluminacion ACTIVADO (F)");
                Debug.Log("+ Resistencia magnetica MEJORADA");
                Debug.Log("+ R.U.B.O. esta casi completo...");
                Debug.Log("========================================");

                // Efecto visual de recogida
                if (pickupRenderer != null)
                {
                    StartCoroutine(CollectEffect());
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private void ShowLockedMessage()
    {
        // Efecto visual de bloqueo
        if (pickupRenderer != null)
        {
            StartCoroutine(LockedFlash());
        }
    }

    private System.Collections.IEnumerator LockedFlash()
    {
        Color lockedColor = Color.red;

        for (int i = 0; i < 3; i++)
        {
            pickupRenderer.material.color = lockedColor;
            yield return new WaitForSeconds(0.1f);
            pickupRenderer.material.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private System.Collections.IEnumerator CollectEffect()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Escala creciente + fade out
            transform.localScale = originalScale * (1 + progress);

            if (pickupRenderer != null)
            {
                Color color = pickupRenderer.material.color;
                color.a = 1 - progress;
                pickupRenderer.material.color = color;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    // Visualizar el rango del trigger en el editor
    private void OnDrawGizmos()
    {
        Gizmos.color = isLocked ? new Color(1f, 0.3f, 0.3f, 0.3f) : new Color(0.2f, 0.7f, 1f, 0.3f);
        Gizmos.DrawWireCube(transform.position, transform.localScale);

        // Dibujar icono
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}