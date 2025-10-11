using UnityEngine;

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

    private Rigidbody rb;
    private bool isGrounded;
    private bool isDashing;
    private float dashTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log("R.U.B.O iniciado - Busca tus PIERNAS para saltar y hacer dash!");
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

    // Método para conectar piernas (llamado desde el pickup)
    public void ConnectLegs()
    {
        hasLegs = true;
        Debug.Log("¡PIERNAS RECONECTADAS! Ahora puedes SALTAR (Space) y hacer DASH (Shift)");
    }

    // Visualizar ground check en editor
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}