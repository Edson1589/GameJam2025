using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Parts Status")]
    public bool hasLegs = false;
    public bool hasArms = false;
    public bool hasTorso = false;

    [Header("Push Settings")]
    [SerializeField] private float pushPower = 3f;
    [SerializeField] private float pushRayDistance = 1.5f;

    [Header("Torso Abilities")]
    [SerializeField] private GameObject flashlight;
    [SerializeField] private float magneticResistance = 0.5f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private TextMeshProUGUI dashCooldownText;
    [SerializeField] private UnityEngine.UI.Slider dashCooldownSlider;
    [SerializeField] private UnityEngine.UI.Image dashFillImage;

    [Header("Skills & Cooldown")]
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float dashCooldownDuration = 10f;

    private int availableJumps;
    private bool isDashAvailable = true;
    private float dashCooldownTimer = 0f;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isDashing;
    private float dashTimer;

    [Header("Camera")]
    [SerializeField] private Transform camTransform;

    [SerializeField] private Animator headAnimator;

    [Header("UI Effects")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseMinScale = 0.98f;
    [SerializeField] private float pulseMaxScale = 1.02f;
    [SerializeField] private Color glowColorMin = new Color(0f, 0.8f, 0f, 1f);
    [SerializeField] private Color glowColorMax = new Color(0.5f, 1f, 0.5f, 1f);

    [Header("Body Parts (Groups or Objects)")]
    [SerializeField] public GameObject headGO;
    [SerializeField] public GameObject legsGroup;
    [SerializeField] public GameObject armsGroup;
    [SerializeField] public GameObject torsoGroup;

    [SerializeField] private PlayerHealth playerHealth;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        if (camTransform == null) camTransform = Camera.main?.transform;

        rb = GetComponent<Rigidbody>();

        availableJumps = maxJumps;

        if (headGO) headGO.SetActive(true);
        if (legsGroup) legsGroup.SetActive(false);
        if (armsGroup) armsGroup.SetActive(false);
        if (torsoGroup) torsoGroup.SetActive(false);

        LoadPartsState();
        InitializeAbilitiesUI();
        if (legsGroup) legsGroup.SetActive(hasLegs);
        if (armsGroup) armsGroup.SetActive(hasArms);
        if (torsoGroup) torsoGroup.SetActive(hasTorso);

        if (playerHealth != null)
        {
            playerHealth.InitializeFromParts(hasLegs, hasArms, hasTorso);
        }

        UpdateStatusText();
        UpdateInstructions();
        Debug.Log("R.U.B.O. (Head) initiated - Find your parts!");
    }

    void Update()
    {
        CheckGrounded();
        HandleInput();

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0) isDashing = false;
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

        UpdateDashCooldownUI();

        if (headAnimator != null)
        {
            float planarSpeed = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;
            headAnimator.SetFloat("Speed", planarSpeed);
        }
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            Move();
        }
        else
        {
            Dash();
        }

        if (hasArms)
        {
            PushObjects();
        }
    }

    private void LoadPartsState()
    {
        if (GameManager.Instance != null)
        {
            hasLegs = GameManager.Instance.hasLegs;
            hasArms = GameManager.Instance.hasArms;
            hasTorso = GameManager.Instance.hasTorso;
        }
    }

    private void HandleInput()
    {
        if (InputManager.Instance != null && InputManager.Instance.GetKeyDown("Jump") && hasLegs && availableJumps > 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            availableJumps--;
            Debug.Log($"Jump! {availableJumps} jumps remaining.");
        }

        if (InputManager.Instance != null && InputManager.Instance.GetKeyDown("Dash") && hasLegs && !isDashing && isDashAvailable)
        {
            isDashing = true;
            dashTimer = dashDuration;
            isDashAvailable = false;
            dashCooldownTimer = dashCooldownDuration;
            Debug.Log("Dash! Cooldown started.");
        }

        if (InputManager.Instance != null && hasTorso && InputManager.Instance.GetKeyDown("Flashlight"))
        {
            ToggleFlashlight();
        }
    }

    private void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDir;
        if (camTransform != null)
        {
            Vector3 camF = camTransform.forward; camF.y = 0f; camF.Normalize();
            Vector3 camR = camTransform.right; camR.y = 0f; camR.Normalize();
            moveDir = (camF * vertical + camR * horizontal).normalized;
        }
        else
        {
            moveDir = new Vector3(horizontal, 0, vertical).normalized;
        }

        float currentSpeed = hasLegs ? moveSpeed : moveSpeed * 0.4f;
        Vector3 movement = moveDir * currentSpeed;
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 12f);
        }
    }

    private void Dash()
    {
        Vector3 dashDir = transform.forward;
        rb.velocity = new Vector3(dashDir.x * dashSpeed, 0, dashDir.z * dashSpeed);
    }

    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (!wasGrounded && isGrounded)
        {
            availableJumps = maxJumps;
            Debug.Log("Jumps reset by touching the ground.");
        }
    }

    private void PushObjects()
    {
        RaycastHit hit;
        Vector3 forward = transform.forward;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(rayStart, forward, out hit, pushRayDistance))
        {
            if (hit.collider.CompareTag("Pushable"))
            {
                Rigidbody boxRb = hit.collider.GetComponent<Rigidbody>();
                if (boxRb != null)
                {
                    float horizontal = Input.GetAxisRaw("Horizontal");
                    float vertical = Input.GetAxisRaw("Vertical");

                    if (horizontal != 0 || vertical != 0)
                    {
                        Vector3 pushDirection = forward;
                        pushDirection.y = 0;
                        boxRb.AddForce(pushDirection * pushPower, ForceMode.Force);
                    }
                }
            }
        }
    }

    private void UpdateDashCooldownUI()
    {
        if (!hasLegs) return;

        if (dashCooldownSlider != null)
        {
            if (isDashAvailable)
            {
                dashCooldownSlider.value = 1f;

                float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

                // Animación de escala
                float scaleMultiplierX = Mathf.Lerp(0.99f, 1.01f, pulse);
                float scaleMultiplierY = Mathf.Lerp(pulseMinScale, pulseMaxScale, pulse);
                dashCooldownSlider.transform.localScale = new Vector3(4f * scaleMultiplierX, 0.6f * scaleMultiplierY, 1f);

                // Animación de brillo
                if (dashFillImage != null)
                {
                    dashFillImage.color = Color.Lerp(glowColorMin, glowColorMax, pulse);
                }
            }
            else
            {
                float progress = (dashCooldownDuration - dashCooldownTimer) / dashCooldownDuration;
                dashCooldownSlider.value = progress;

                dashCooldownSlider.transform.localScale = new Vector3(4f, 0.6f, 1f);

                if (dashFillImage != null)
                    dashFillImage.color = Color.yellow;
            }
        }

        if (dashCooldownText != null)
        {
            if (isDashAvailable)
            {
                dashCooldownText.text = "DASH: READY";
                dashCooldownText.color = Color.green;
            }
            else
            {
                float timeElapsed = dashCooldownDuration - dashCooldownTimer;
                if (timeElapsed < 0) timeElapsed = 0;
                dashCooldownText.text = $"DASH: {timeElapsed:0.0}s";
                dashCooldownText.color = Color.yellow;
            }
        }
    }

    private void InitializeAbilitiesUI()
    {
        if (dashCooldownText != null)
        {
            dashCooldownText.gameObject.SetActive(hasLegs);
            if (hasLegs)
            {
                UpdateDashCooldownUI();
            }
        }

        if (dashCooldownSlider != null)
        {
            dashCooldownSlider.gameObject.SetActive(hasLegs);
            if (hasLegs)
            {
                dashCooldownSlider.transform.localScale = new Vector3(4f, 0.6f, 1f);
                dashCooldownSlider.value = 1f;
                if (dashFillImage != null)
                    dashFillImage.color = glowColorMin;
            }
        }
    }

    public void ConnectLegs()
    {
        hasLegs = true;
        if (legsGroup) legsGroup.SetActive(true);

        if (dashCooldownText != null)
        {
            dashCooldownText.gameObject.SetActive(true);
            UpdateDashCooldownUI();
        }

        if (dashCooldownSlider != null)
        {
            dashCooldownSlider.gameObject.SetActive(true);
            dashCooldownSlider.value = 1f;
            if (dashFillImage != null)
            {
                Color colorHex;
                if (ColorUtility.TryParseHtmlString("#28E2ED", out colorHex))
                    dashFillImage.color = colorHex;
            }

        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectLegs();
        }

        if (playerHealth != null) playerHealth.OnPartConnected(BodyPart.Legs);
        UpdateStatusText();
        UpdateInstructions();
        Debug.Log("PIERNAS RECONECTADAS! Salto y Dash desbloqueados");
    }

    public void ConnectArms()
    {
        hasArms = true;
        if (armsGroup) armsGroup.SetActive(true);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectArms();
        }

        if (playerHealth != null) playerHealth.OnPartConnected(BodyPart.Arms);
        UpdateStatusText();
        UpdateInstructions();
        Debug.Log("BRAZOS RECONECTADOS! Empujar cajas y usar palancas disponible");
    }

    public void ConnectTorso()
    {
        hasTorso = true;
        if (torsoGroup) torsoGroup.SetActive(true);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectTorso();
        }

        if (playerHealth != null) playerHealth.OnPartConnected(BodyPart.Torso);
        UpdateStatusText();
        UpdateInstructions();

        if (flashlight != null)
        {
            flashlight.SetActive(false);
        }

        Debug.Log("TORSO RECONECTADO! Ensamblaje completo - Linterna disponible");
    }

    public bool IsFullyAssembled()
    {
        return hasLegs && hasArms && hasTorso;
    }

    public float GetMagneticResistance()
    {
        return hasTorso ? magneticResistance : 0f;
    }

    private void ToggleFlashlight()
    {
        if (flashlight != null)
        {
            flashlight.SetActive(!flashlight.activeSelf);
            Debug.Log($"Linterna: {(flashlight.activeSelf ? "ON" : "OFF")}");
        }
    }

    private void UpdateInstructions()
    {
        if (instructionsText == null) return;

        if (!hasLegs)
        {
            instructionsText.text = "LEVEL 1: Assembly Zone - Find your LEGS";
            instructionsText.color = new Color(1f, 0.3f, 0.3f);
        }
        else if (!hasArms)
        {
            instructionsText.text = "LEVEL 2: Tapes and Packaging - Find your ARMS";
            instructionsText.color = new Color(1f, 0.8f, 0.2f);
        }
        else if (!hasTorso)
        {
            instructionsText.text = "LEVEL 3: Rooftop-Heliport - Find your TORSO";
            instructionsText.color = new Color(0.3f, 0.8f, 1f);
        }
        else
        {
            instructionsText.text = "COMPLETE ASSEMBLY! - Head to the EXIT";
            instructionsText.color = new Color(0.3f, 1f, 0.3f);
        }
    }

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            string status = "";

            if (hasLegs && hasArms && hasTorso)
            {
                status = "FULLY OPERATIONAL";
                statusText.color = Color.cyan;
            }
            else if (hasLegs && hasArms)
            {
                status = "LEGS + ARMS - Find the TORSO";
                statusText.color = Color.yellow;
            }
            else if (hasLegs)
            {
                status = "LEGS - Jump & Dash Available";
                statusText.color = Color.green;
            }
            else if (hasArms)
            {
                status = "ARMS - You can push boxes";
                statusText.color = Color.yellow;
            }
            else
            {
                status = "NO PARTS - Limited movement";
                statusText.color = Color.red;
            }

            statusText.text = status;
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (hasArms)
        {
            Gizmos.color = Color.yellow;
            Vector3 rayStart = transform.position + Vector3.up * 0.5f;
            Gizmos.DrawRay(rayStart, transform.forward * pushRayDistance);
        }
    }
}