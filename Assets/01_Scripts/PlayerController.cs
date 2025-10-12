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

    [Header("Push Settings")]
    [SerializeField] private float pushPower = 3f;
    [SerializeField] private float pushRayDistance = 1.5f;

    [Header("UI (Optional)")]
    [SerializeField] private TextMeshProUGUI statusText;

    // Components
    private Rigidbody rb;
    private bool isGrounded;
    private bool isDashing;
    private float dashTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        UpdateStatusText();
        Debug.Log("R.U.B.O. (Cabeza) iniciado - Busca tus partes!");
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

        // Empujar objetos si tiene brazos
        if (hasArms)
        {
            PushObjects();
        }
    }

    private void HandleInput()
    {
        // Jump (Space) - Solo si tiene piernas
        if (Input.GetKeyDown(KeyCode.Space) && hasLegs && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            Debug.Log("¡Salto!");
        }

        // Dash (Shift) - Solo si tiene piernas
        if (Input.GetKeyDown(KeyCode.LeftShift) && hasLegs && !isDashing && isGrounded)
        {
            isDashing = true;
            dashTimer = dashDuration;
            Debug.Log("¡Dash!");
        }
    }

    private void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 moveDir = new Vector3(horizontal, 0, vertical).normalized;

        // Más lento sin piernas
        float currentSpeed = hasLegs ? moveSpeed : moveSpeed * 0.4f;

        Vector3 movement = moveDir * currentSpeed;
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

        // Rotar hacia donde mira
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
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void PushObjects()
    {
        // Raycast hacia adelante para detectar objetos empujables
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
                    // Empujar si nos estamos moviendo hacia el objeto
                    float horizontal = Input.GetAxisRaw("Horizontal");
                    float vertical = Input.GetAxisRaw("Vertical");

                    if (horizontal != 0 || vertical != 0)
                    {
                        Vector3 pushDirection = forward;
                        pushDirection.y = 0; // Solo empujar horizontalmente
                        boxRb.AddForce(pushDirection * pushPower, ForceMode.Force);
                    }
                }
            }
        }
    }

    // Método para conectar piernas
    public void ConnectLegs()
    {
        hasLegs = true;
        UpdateStatusText();
        Debug.Log("¡PIERNAS RECONECTADAS! Ahora puedes SALTAR (Space) y hacer DASH (Shift)");
    }

    // Método para conectar brazos
    public void ConnectArms()
    {
        hasArms = true;
        UpdateStatusText();
        Debug.Log("¡BRAZOS RECONECTADOS! Ahora puedes EMPUJAR/ARRASTRAR cajas y usar PALANCAS");
    }

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            string status = "";

            if (hasLegs && hasArms)
            {
                status = "PIERNAS + BRAZOS - Totalmente operativo";
                statusText.color = Color.cyan;
            }
            else if (hasLegs)
            {
                status = "PIERNAS - Space: Saltar | Shift: Dash";
                statusText.color = Color.green;
            }
            else if (hasArms)
            {
                status = "BRAZOS - Puedes empujar cajas";
                statusText.color = Color.yellow;
            }
            else
            {
                status = "SIN PARTES - Movimiento limitado";
                statusText.color = Color.red;
            }

            statusText.text = status;
        }
    }

    // Visualizar ground check y push ray en editor
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Visualizar rayo de empuje
        if (hasArms)
        {
            Gizmos.color = Color.yellow;
            Vector3 rayStart = transform.position + Vector3.up * 0.5f;
            Gizmos.DrawRay(rayStart, transform.forward * pushRayDistance);
        }
    }
}