using UnityEngine;
using UnityEngine.UI;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldownDuration = 10f;

    [Header("UI References")]
    [SerializeField] private Image dashFillImage;

    private PlayerController playerController;
    private Rigidbody rb;
    private GameObject dashUIRoot;
    private Color dashColorReady = new Color(31f / 255f, 218f / 255f, 233f / 255f);
    private Color dashColorCooldown = Color.gray;

    private bool isDashAvailable = true;
    private float dashCooldownTimer = 0f;
    private bool isDashing = false;
    private float dashTimer;

    public bool IsDashing => isDashing;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();

        Debug.Log("=== PlayerDash Awake ===");

        if (dashUIRoot == null && dashFillImage != null)
        {
            dashUIRoot = dashFillImage.transform.parent?.gameObject;

            if (dashUIRoot != null)
            {
                Debug.Log($"✓ dashUIRoot encontrado automáticamente: {dashUIRoot.name}");
            }
            else
            {
                Debug.LogError("✗ No se pudo encontrar el padre de dashFillImage!");
            }
        }

        if (dashUIRoot != null)
        {
            dashUIRoot.SetActive(false);
            Debug.Log($"✓ Dash UI '{dashUIRoot.name}' desactivado en Awake");
        }
        else
        {
            Debug.LogError("✗ ERROR: dashUIRoot es NULL después de intentar auto-asignarlo!");
        }
    }

    public void InitializeUI(bool hasLegs)
    {
        if (hasLegs)
        {
            if (dashUIRoot != null)
            {
                dashUIRoot.SetActive(true);
            }
            isDashAvailable = true;
        }
        UpdateDashUI();
    }

    void Update()
    {
        if (playerController.hasLegs)
        {
            HandleDashInput();
            HandleDashTimerAndCooldown();
            UpdateDashUI();
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            DashMovement();
        }
    }

    private void HandleDashInput()
    {
        if (InputManager.Instance == null)
        {
            return;
        }

        if (InputManager.Instance.GetKeyDown("Dash") && !isDashing && isDashAvailable)
        {
            StartDash();
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        isDashAvailable = false;
        dashCooldownTimer = dashCooldownDuration;

        if (dashFillImage != null)
        {
            dashFillImage.color = dashColorCooldown;
        }

        Debug.Log("Dash! Cooldown started.");
    }

    private void HandleDashTimerAndCooldown()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
            }
        }

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;

            if (dashCooldownTimer <= 0)
            {
                Debug.Log("DASH RECHARGED!");
                isDashAvailable = true;
                dashCooldownTimer = 0f;
            }
        }
    }

    private void DashMovement()
    {
        Vector3 dashDir = transform.forward;
        rb.velocity = new Vector3(dashDir.x * dashSpeed, 0, dashDir.z * dashSpeed);
    }

    private void UpdateDashUI()
    {
        if (dashFillImage == null || !playerController.hasLegs) return;

        // Verificar que no sea el mismo Image que la barra de vida
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.GetHealthFillImage() == dashFillImage)
        {
           
            return;
        }

        if (isDashAvailable)
        {
            dashFillImage.fillAmount = 1f;
            dashFillImage.color = dashColorReady;
        }
        else
        {
            float progress = (dashCooldownDuration - dashCooldownTimer) / dashCooldownDuration;
            dashFillImage.fillAmount = Mathf.Clamp01(progress);
            dashFillImage.color = dashColorCooldown;
        }
    }

    public void OnLegsConnected()
    {
        if (dashUIRoot != null)
        {
            dashUIRoot.SetActive(true);
        }
        isDashAvailable = true;
        UpdateDashUI();
    }
}