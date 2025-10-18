using UnityEngine;

public class BossPanel : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private AnchorMother boss;
    [SerializeField] private bool isActivated = false;

    [Header("Visual")]
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private Material activeMaterial;
    [SerializeField] private GameObject interactionPrompt;

    private Renderer panelRenderer;
    private bool playerNearby = false;

    void Start()
    {
        panelRenderer = GetComponent<Renderer>();
        UpdateVisual();

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (boss == null)
        {
            Debug.LogWarning($"BossPanel '{gameObject.name}' no tiene boss asignado!");
        }
    }

    void Update()
    {
        if (playerNearby && !isActivated && Input.GetKeyDown(KeyCode.E))
        {
            ActivatePanel();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            playerNearby = true;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }

            Debug.Log("Presiona E para activar panel");
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

    private void ActivatePanel()
    {
        isActivated = true;
        UpdateVisual();

        if (boss != null)
        {
            boss.RegisterPanelActivation();
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        Debug.Log($"Panel '{gameObject.name}' ACTIVADO!");
    }

    private void UpdateVisual()
    {
        if (panelRenderer != null)
        {
            panelRenderer.material = isActivated ? activeMaterial : inactiveMaterial;
        }
    }
}