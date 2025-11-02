using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;

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

    [Header("Skills")]
    [SerializeField] private int maxJumps = 2;

    private int availableJumps;
    private Rigidbody rb;
    private bool isGrounded;

    [Header("Camera")]
    [SerializeField] private Transform camTransform;

    [Header("UI Effects")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseMinScale = 0.98f;
    [SerializeField] private float pulseMaxScale = 1.02f;

    [Header("Body Parts (Groups or Objects)")]
    [SerializeField] public GameObject headGO;
    [SerializeField] public GameObject legsGroup;
    [SerializeField] public GameObject armsGroup; 
    [SerializeField] public GameObject torsoGroup;
    [SerializeField] private Transform headGroupTransform;
    [SerializeField] private Transform torsoGroupTransform;

    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerDash playerDash;

    [Header("Height Adjustment System")]

    [SerializeField] private float headOnlyColliderHeight = 0.3f;
    [SerializeField] private float withTorsoColliderHeight = 0.6f;
    [SerializeField] private float withLegsColliderHeight = 1f;

    [SerializeField] private float headStackingOffsetTorso = -0.15f;
    [SerializeField] private float headStackingOffsetLegs = -0.13f;
    [SerializeField] private float torsoStackingOffsetLegs = 0.42f;
    [SerializeField] private float legsStackingOffset = 0.468f;
    [SerializeField] private float armsStackingOffset = 0.46f;

    [SerializeField] private float heightTransitionDuration = 0.5f;

    private CapsuleCollider capsuleCollider;
    private bool isTransitioningHeight = false;

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
        capsuleCollider = GetComponent<CapsuleCollider>();
        availableJumps = maxJumps;

        LoadPartsState();

        if (legsGroup) legsGroup.SetActive(hasLegs);
        if (armsGroup) armsGroup.SetActive(hasArms);
        if (torsoGroup) torsoGroup.SetActive(hasTorso);

        if (playerDash != null)
        {
            playerDash.InitializeUI(hasLegs);
        }

        BodyConfig initialConfig = GetCurrentBodyConfig();

        UpdateColliderAndPivot(initialConfig);

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        UpdateStatusText();
        UpdateInstructions();
    }

    void Update()
    {
        CheckGrounded();
        HandleInput();
    }

    void FixedUpdate()
    {
        if (playerDash == null || !playerDash.IsDashing)
        {
            Move();
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

    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (!wasGrounded && isGrounded)
        {
            availableJumps = maxJumps;
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

    private BodyConfig GetCurrentBodyConfig()
    {
        if (hasLegs) return BodyConfig.WithLegs;
        if (hasTorso) return BodyConfig.HeadAndTorso;
        return BodyConfig.HeadOnly;
    }


    private float GetHeightForConfig(BodyConfig config)
    {
        float colliderHeight = 0f;
        switch (config)
        {
            case BodyConfig.HeadOnly: colliderHeight = headOnlyColliderHeight; break;
            case BodyConfig.HeadAndTorso: colliderHeight = withTorsoColliderHeight; break;
            case BodyConfig.WithLegs: colliderHeight = withLegsColliderHeight; break;
        }
        return colliderHeight * 0.5f;
    }

    private void AdjustHeightImmediate(BodyConfig config)
    {
        float targetHeight = GetHeightForConfig(config);

        Vector3 targetPos = new Vector3(transform.position.x, targetHeight, transform.position.z);

        if (rb != null)
        {
            rb.position = targetPos;
        }
        else
        {
            transform.position = targetPos;
        }

        UpdateColliderAndPivot(config);
    }

    private IEnumerator TransitionHeight(BodyConfig newConfig)
    {
        if (isTransitioningHeight) yield break;
        isTransitioningHeight = true;

        if (rb != null) rb.isKinematic = true;

        float startHeight = transform.position.y;
        float endHeight = GetHeightForConfig(newConfig);

        float elapsed = 0f;

        UpdateColliderAndPivot(newConfig);

        while (elapsed < heightTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / heightTransitionDuration);

            float newY = Mathf.Lerp(startHeight, endHeight, t);
            Vector3 targetPos = new Vector3(transform.position.x, newY, transform.position.z);

            transform.position = targetPos;

            yield return null;
        }

        Vector3 finalPos = new Vector3(transform.position.x, endHeight, transform.position.z);
        transform.position = finalPos;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        isTransitioningHeight = false;
    }

    private void UpdateColliderAndPivot(BodyConfig config)
    {
        if (capsuleCollider == null) return;

        float targetHeight = 0f;
        switch (config)
        {
            case BodyConfig.HeadOnly: targetHeight = headOnlyColliderHeight; break;
            case BodyConfig.HeadAndTorso: targetHeight = withTorsoColliderHeight; break;
            case BodyConfig.WithLegs: targetHeight = withLegsColliderHeight; break;
        }

        capsuleCollider.height = targetHeight;
        capsuleCollider.center = new Vector3(0, targetHeight * 0.5f, 0);

        if (headGroupTransform != null)
        {
            float localHeadY = 0f;
            float headBasePosition = targetHeight;
            float halfHead = headOnlyColliderHeight * 0.5f;
            float currentOffset = 0f;

            switch (config)
            {
                case BodyConfig.HeadOnly:
                    localHeadY = 0f;
                    currentOffset = 0f;
                    break;
                case BodyConfig.HeadAndTorso:
                    currentOffset = headStackingOffsetTorso;
                    localHeadY = headBasePosition - halfHead + currentOffset;
                    break;
                case BodyConfig.WithLegs:
                    currentOffset = headStackingOffsetLegs;
                    localHeadY = headBasePosition - halfHead + currentOffset;
                    break;
            }

            headGroupTransform.localPosition = new Vector3(0, localHeadY, 0);
        }

        if (torsoGroupTransform != null)
        {
            float localTorsoY = 0f;

            switch (config)
            {
                case BodyConfig.HeadOnly:
                case BodyConfig.HeadAndTorso:
                    localTorsoY = 0f;
                    break;
                case BodyConfig.WithLegs:
                    localTorsoY = torsoStackingOffsetLegs;
                    break;
            }

            torsoGroupTransform.localPosition = new Vector3(0, localTorsoY, 0);
        }

        if (legsGroup != null)
        {
            float localLegsY = 0f;

            if (config == BodyConfig.WithLegs)
            {
                localLegsY = legsStackingOffset;
            }

            legsGroup.transform.localPosition = new Vector3(0, localLegsY, 0);
        }

        if (armsGroup != null)
        {
            float localArmsY = 0f;

            if (hasArms) 
            {
                localArmsY = armsStackingOffset;
            }

            armsGroup.transform.localPosition = new Vector3(0, localArmsY, 0);
        }

        if (groundCheck != null)
        {
            groundCheck.localPosition = new Vector3(0, -(targetHeight * 0.5f), 0);
        }
    }

    public void ConnectLegs()
    {
        hasLegs = true;
        if (legsGroup) legsGroup.SetActive(true);

        BodyConfig newConfig = GetCurrentBodyConfig();

        if (hasTorso)
        {
            AdjustHeightImmediate(newConfig);
            StartCoroutine(TransitionHeight(newConfig));
        }
        else
        {
            StartCoroutine(TransitionHeight(newConfig));
        }

        if (playerDash != null)
        {
            playerDash.OnLegsConnected();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectLegs();
        }

        UpdateStatusText();
        UpdateInstructions();
    }

    public void ConnectArms()
    {
        hasArms = true;
        if (armsGroup) armsGroup.SetActive(true);

        UpdateColliderAndPivot(GetCurrentBodyConfig());

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectArms();
        }

        UpdateStatusText();
        UpdateInstructions();
    }

    public void ConnectTorso()
    {
        hasTorso = true;
        if (torsoGroup) torsoGroup.SetActive(true);

        BodyConfig newConfig = GetCurrentBodyConfig();

        if (!hasLegs)
        {
            StartCoroutine(TransitionHeight(newConfig));
        }
        else
        {
            UpdateColliderAndPivot(newConfig);
        }

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
        }
    }

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            string status = $"Status:\n";
            status += $"Head: <color=green>Connected</color>\n";
            status += $"Torso: {(hasTorso ? "<color=green>Connected</color>" : "<color=red>Missing</color>")}\n";
            status += $"Arms: {(hasArms ? "<color=green>Connected</color>" : "<color=red>Missing</color>")}\n";
            status += $"Legs: {(hasLegs ? "<color=green>Connected</color>" : "<color=red>Missing</color>")}\n";

            if (playerHealth != null)
            {
                int currentHP = playerHealth.GetCurrentHP();
                int ownedMaxHP = playerHealth.GetOwnedMax();
                status += $"Health: {currentHP} / {ownedMaxHP}";
            }

            statusText.text = status;
        }
    }

    private void UpdateInstructions()
    {
        if (instructionsText != null)
        {
            string instructions = "Controls:\n";
            instructions += "- WASD/Arrows: Move\n";

            if (hasLegs)
            {
                instructions += $"- Space: Jump ({availableJumps} / {maxJumps})\n";
                if (playerDash != null)
                {
                    instructions += $"- Left Shift: Dash\n";
                }
            }
            else
            {
                instructions += "- Movement is slow without legs.\n";
            }

            if (hasTorso)
            {
                instructions += "- F: Toggle Flashlight\n";
            }

            if (hasArms)
            {
                instructions += "- Automatically pushes objects in front.\n";
            }

            instructionsText.text = instructions;
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

public enum BodyConfig
{
    HeadOnly,
    HeadAndTorso,
    WithLegs
}