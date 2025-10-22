﻿using UnityEngine;
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

    [Header("Torso Abilities")]
    [SerializeField] private GameObject flashlight;
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
    private bool isGrounded;
    private bool isDashing;
    private float dashTimer;

    [Header("Camera")]
    [SerializeField] private Transform camTransform;

    [SerializeField] private Animator headAnimator;

    [Header("Body Parts (Groups or Objects)")]
    [SerializeField] private GameObject headGO;
    [SerializeField] public GameObject legsGroup;
    [SerializeField] public GameObject armsGroup;
    [SerializeField] public GameObject torsoGroup;

    void Start()
    {
        if (camTransform == null) camTransform = Camera.main?.transform;

        rb = GetComponent<Rigidbody>();

        availableJumps = maxJumps;

        if (headGO) headGO.SetActive(true);
        if (legsGroup) legsGroup.SetActive(false);
        if (armsGroup) armsGroup.SetActive(false);
        if (torsoGroup) torsoGroup.SetActive(false);

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
                if (availableJumps < maxJumps)
                {
                    availableJumps = maxJumps;
                }
                cooldownTimer = 0;
            }
        }

        if (headAnimator != null)
        {
            float planarSpeed = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;
            headAnimator.SetFloat("Speed", planarSpeed);
        }

        // Toggle flashlight con F si tiene torso
        if (hasTorso && Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
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

    public void ConnectLegs()
    {
        hasLegs = true;
        if (legsGroup) legsGroup.SetActive(true);

        // ¡GUARDAR EN GAMEMANAGER!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectLegs();
        }

        UpdateStatusText();
        UpdateInstructions();
        Debug.Log("PIERNAS RECONECTADAS! Salto (Space) y Dash (Shift) desbloqueados");
    }

    public void ConnectArms()
    {
        hasArms = true;
        if (armsGroup) armsGroup.SetActive(true);

        // ¡GUARDAR EN GAMEMANAGER!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectArms();
        }

        UpdateStatusText();
        UpdateInstructions();
        Debug.Log("BRAZOS RECONECTADOS! Empujar cajas y usar palancas disponible");
    }

    public void ConnectTorso()
    {
        hasTorso = true;
        if (torsoGroup) torsoGroup.SetActive(true);

        // ¡GUARDAR EN GAMEMANAGER!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectTorso();
        }

        UpdateStatusText();
        UpdateInstructions();

        if (flashlight != null)
        {
            flashlight.SetActive(false);
        }

        Debug.Log("TORSO RECONECTADO! Ensamblaje completo - Presiona F para linterna");
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