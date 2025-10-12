using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
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

    [Header("Torso Settings")]
    [SerializeField] private Light playerLight;
    [SerializeField] private float magneticResistance = 0.5f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI instructionsText;

    [Header("Skills & Cooldown")]
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float abilityCooldownDuration = 10f;

    private int availableJumps;
    private bool isDashAvailable = true;
    private float cooldownTimer;

    private Rigidbody rb;
    private BodyPartsVisualizer bodyVisualizer;
    private bool isGrounded;
    private bool isDashing;
    private float dashTimer;
    private bool lightEnabled = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        bodyVisualizer = GetComponent<BodyPartsVisualizer>();

        availableJumps = maxJumps;

        if (bodyVisualizer != null)
        {
            Debug.Log("=== BodyPartsVisualizer found! ===");
        }
        else
        {
            Debug.LogError("=== BodyPartsVisualizer NOT found! Add it to the Player ===");
        }

        if (playerLight != null)
            playerLight.enabled = false;

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

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                Debug.Log("ABILITIES RECHARGED!");
                isDashAvailable = true;
                // Saltos se resetean al entrar en contacto con el piso.
                if (availableJumps < maxJumps)
                {
                    availableJumps = maxJumps;
                }
                cooldownTimer = 0;
            }
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

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && hasLegs && availableJumps > 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            availableJumps--;
            Debug.Log($"Jump! {availableJumps} jumps remaining.");

            if (cooldownTimer <= 0)
            {
                cooldownTimer = abilityCooldownDuration;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && hasLegs && !isDashing && isDashAvailable)
        {
            isDashing = true;
            dashTimer = dashDuration;
            isDashAvailable = false;
            Debug.Log("Dash! Cooldown started.");

            if (cooldownTimer <= 0)
            {
                cooldownTimer = abilityCooldownDuration;
            }
        }

        if (Input.GetKeyDown(KeyCode.F) && hasTorso)
        {
            ToggleLight();
        }
    }

    private void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 moveDir = new Vector3(horizontal, 0, vertical).normalized;

        float currentSpeed = hasLegs ? moveSpeed : moveSpeed * 0.4f;

        Vector3 movement = moveDir * currentSpeed;
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
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

    private void ToggleLight()
    {
        if (playerLight == null) return;

        lightEnabled = !lightEnabled;
        playerLight.enabled = lightEnabled;

        Debug.Log(lightEnabled ? "Flashlight ENABLED" : "Flashlight DISABLED");
    }

    public void ConnectLegs()
    {
        Debug.Log(">>> ConnectLegs() called");
        hasLegs = true;

        // Verificar y acoplar visualmente
        if (bodyVisualizer != null)
        {
            Debug.Log(">>> bodyVisualizer exists, calling AttachLegs()...");
            bodyVisualizer.AttachLegs();
        }
        else
        {
            Debug.LogError(">>> bodyVisualizer is NULL! Cannot attach visually");
            Debug.LogError(">>> Make sure the BodyPartsVisualizer component is on the Player");
        }

        UpdateStatusText();
        UpdateInstructions();
        Debug.Log("LEGS RECONNECTED! Jump (Space) and Dash (Shift) unlocked");
    }

    public void ConnectArms()
    {
        Debug.Log(">>> ConnectArms() called");
        hasArms = true;

        if (bodyVisualizer != null)
        {
            Debug.Log(">>> bodyVisualizer exists, calling AttachArms()...");
            bodyVisualizer.AttachArms();
        }
        else
        {
            Debug.LogError(">>> bodyVisualizer is NULL! Cannot attach visually");
        }

        UpdateStatusText();
        UpdateInstructions();
        Debug.Log("ARMS RECONNECTED! Pushing boxes and using levers is available");
    }

    public void ConnectTorso()
    {
        Debug.Log(">>> ConnectTorso() called");
        hasTorso = true;

        if (bodyVisualizer != null)
        {
            Debug.Log(">>> bodyVisualizer exists, calling AttachTorso()...");
            bodyVisualizer.AttachTorso();
        }
        else
        {
            Debug.LogError(">>> bodyVisualizer is NULL! Cannot attach visually");
        }

        if (playerLight != null)
        {
            lightEnabled = true;
            playerLight.enabled = true;
        }

        UpdateStatusText();
        UpdateInstructions();
        Debug.Log("TORSO RECONNECTED! Flashlight/Visor (F) and Magnetic Resistance active");
    }

    public bool IsFullyAssembled()
    {
        return hasLegs && hasArms && hasTorso;
    }

    public float GetMagneticResistance()
    {
        return hasTorso ? magneticResistance : 0f;
    }

    private void UpdateInstructions()
    {
        if (instructionsText == null) return;

        if (!hasLegs && !hasArms && !hasTorso)
        {
            instructionsText.text = "LEVEL 1: Assembly Zone - Find your LEGS";
            instructionsText.color = new Color(1f, 0.4f, 0.4f);
        }
        else if (hasLegs && !hasArms && !hasTorso)
        {
            instructionsText.text = "LEVEL 2: Tapes & Packaging - Find your ARMS";
            instructionsText.color = new Color(1f, 0.8f, 0.2f);
        }
        else if (hasLegs && hasArms && !hasTorso)
        {
            instructionsText.text = "LEVEL 3: Rooftop-Helipad - Find your TORSO";
            instructionsText.color = new Color(0.4f, 0.8f, 1f);
        }
        else if (hasLegs && hasArms && hasTorso)
        {
            instructionsText.text = "ASSEMBLY COMPLETE - Deactivate the MOTHER ANCHOR";
            instructionsText.color = new Color(0.2f, 1f, 0.4f);
        }
        else
        {
            instructionsText.text = "Find the missing parts of R.U.B.O.";
            instructionsText.color = new Color(1f, 0.6f, 0.2f);
        }
    }

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            string status = "";

            if (hasLegs && hasArms && hasTorso)
            {
                status = "FULLY OPERATIONAL - F: Flashlight";
                statusText.color = Color.cyan;
            }
            else if (hasLegs && hasArms)
            {
                status = "LEGS + ARMS - Find the TORSO";
                statusText.color = Color.yellow;
            }
            else if (hasLegs)
            {
                status = "LEGS - Space: Jump | Shift: Dash";
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