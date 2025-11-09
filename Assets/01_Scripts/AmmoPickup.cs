using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AmmoPickup : MonoBehaviour
{
    [Header("Ammo Settings")]
    [SerializeField] private int ammoAmount = 1; // Cantidad de munición que da este pickup
    [SerializeField] private bool respawnAfterTime = false;
    [SerializeField] private float respawnTime = 10f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private GameObject visualObject; // Objeto visual que se desactiva al recoger
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatAmplitude = 0.3f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 0.7f;
    private AudioSource audioSource;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 2f; // Rango para detectar al jugador
    [SerializeField] private bool autoPickup = true; // Se recoge automáticamente al acercarse

    private Vector3 startPosition;
    private float floatOffset = 0f;
    private bool isCollected = false;
    private Collider pickupCollider;

    private void Awake()
    {
        pickupCollider = GetComponent<Collider>();
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }

        startPosition = transform.position;

        if (pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 20f;
        }

        // Si no hay objeto visual asignado, usar el objeto principal
        if (visualObject == null)
        {
            visualObject = gameObject;
        }
    }

    private void Update()
    {
        if (isCollected) return;

        // Rotación del objeto
        if (visualObject != null)
        {
            visualObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        // Movimiento flotante
        floatOffset += floatSpeed * Time.deltaTime;
        float verticalOffset = Mathf.Sin(floatOffset) * floatAmplitude;
        transform.position = startPosition + Vector3.up * verticalOffset;

        // Auto-pickup si está cerca del jugador
        if (autoPickup)
        {
            CheckPlayerProximity();
        }
    }

    private void CheckPlayerProximity()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= detectionRange)
            {
                CollectAmmo(player);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            CollectAmmo(other.gameObject);
        }
    }

    private void CollectAmmo(GameObject player)
    {
        if (isCollected) return;

        // Obtener el sistema de munición del jugador
        PlayerAmmoSystem ammoSystem = player.GetComponent<PlayerAmmoSystem>();
        if (ammoSystem == null)
        {
            ammoSystem = player.GetComponentInParent<PlayerAmmoSystem>();
        }

        if (ammoSystem != null)
        {
            ammoSystem.AddAmmo(ammoAmount);
            isCollected = true;

            // Efecto visual
            if (pickupEffect != null)
            {
                GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // Sonido
            if (pickupSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pickupSound, soundVolume);
            }

            // Desactivar visual
            if (visualObject != null)
            {
                visualObject.SetActive(false);
            }

            // Desactivar collider
            if (pickupCollider != null)
            {
                pickupCollider.enabled = false;
            }

            // Respawn si está configurado
            if (respawnAfterTime)
            {
                Invoke("Respawn", respawnTime);
            }
            else
            {
                Destroy(gameObject, 0.5f); // Delay para que el sonido se reproduzca
            }
        }
    }

    // Método público para respawn manual (cuando el jugador respawnea)
    public void Respawn()
    {
        if (!isCollected) return; // Si no está recogido, no hacer nada

        isCollected = false;
        if (visualObject != null)
        {
            visualObject.SetActive(true);
        }
        
        if (pickupCollider != null)
        {
            pickupCollider.enabled = true;
        }
        
        transform.position = startPosition; // Resetear posición para el flotamiento
        floatOffset = Random.Range(0f, 2f * Mathf.PI); // Nuevo offset aleatorio
        
        Debug.Log($"AmmoPickup: Respawned at {transform.position}");
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar rango de detección
        if (autoPickup)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}

