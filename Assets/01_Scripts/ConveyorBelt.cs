using UnityEngine;
using System.Collections.Generic;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Belt Settings")]
    [SerializeField] private Vector3 direction = Vector3.forward;
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool isActive = true;

    [Header("Visual Feedback")]
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private float textureScrollSpeed = 0.5f;

    private Renderer beltRenderer;
    private List<Rigidbody> objectsOnBelt = new List<Rigidbody>();
    private float textureOffset = 0f;

    void Start()
    {
        beltRenderer = GetComponent<Renderer>();
        UpdateVisual();
    }

    void Update()
    {
        // Animar textura si está activa
        if (isActive && beltRenderer != null)
        {
            textureOffset += textureScrollSpeed * Time.deltaTime;
            beltRenderer.material.mainTextureOffset = new Vector2(textureOffset, 0);
        }
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        // Mover todos los objetos en la cinta
        foreach (Rigidbody rb in objectsOnBelt)
        {
            if (rb != null)
            {
                Vector3 force = direction.normalized * speed * 10f; // Multiplicar por masa efectiva
                rb.AddForce(force, ForceMode.Force);
            }
        }
    }

    // Usar triggers para detección más confiable
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb != null && !objectsOnBelt.Contains(rb))
        {
            objectsOnBelt.Add(rb);
            Debug.Log($"Objeto '{other.gameObject.name}' entró en cinta");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Mantener objetos en la lista mientras están encima
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb != null && !objectsOnBelt.Contains(rb))
        {
            objectsOnBelt.Add(rb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb != null && objectsOnBelt.Contains(rb))
        {
            objectsOnBelt.Remove(rb);
            Debug.Log($"Objeto '{other.gameObject.name}' salió de cinta");
        }
    }

    // Métodos públicos para control externo
    public void Activate()
    {
        isActive = true;
        UpdateVisual();
        Debug.Log($"Cinta '{gameObject.name}' ACTIVADA");
    }

    public void Deactivate()
    {
        isActive = false;
        UpdateVisual();
        Debug.Log($"Cinta '{gameObject.name}' DESACTIVADA");
    }

    public void ReverseDirection()
    {
        direction = -direction;
        Debug.Log($"Cinta '{gameObject.name}' cambió dirección a {direction}");
    }

    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection.normalized;
    }

    private void UpdateVisual()
    {
        if (beltRenderer != null)
        {
            if (isActive && activeMaterial != null)
            {
                beltRenderer.material = activeMaterial;
            }
            else if (!isActive && inactiveMaterial != null)
            {
                beltRenderer.material = inactiveMaterial;
            }
        }
    }

    // Visualización en editor
    private void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.cyan : Color.gray;
        Vector3 center = transform.position + Vector3.up * 0.1f;
        Vector3 arrowEnd = center + direction.normalized * 2f;

        // Dibujar flecha de dirección
        Gizmos.DrawLine(center, arrowEnd);
        Gizmos.DrawSphere(arrowEnd, 0.2f);
    }
}