using UnityEngine;
using UnityEngine.UI;

public class PusherBot : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Homing (seguimiento)")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float turnDegPerSec = 540f;
    [SerializeField] private bool followPlayerY = true;
    [SerializeField] private float verticalFollowLerp = 8f;

    [Header("Empuje y Daño")]
    [SerializeField] private float pushImpulse = 8f;
    [SerializeField] private int damageOnTouch = 1;

    [Header("Vida útil y UI")]
    [SerializeField] private float lifeSeconds = 6f;
    [SerializeField] private Image lifeCircle;

    private float remaining;
    private Vector3 forwardDir;
    private PusherBotSpawner spawner;

    public void Init(Transform playerRef, float lifetimeOverride, PusherBotSpawner owner)
    {
        player = playerRef;
        spawner = owner;
        remaining = (lifetimeOverride > 0f) ? lifetimeOverride : lifeSeconds;

        Vector3 dir = transform.forward;
        if (player)
        {
            dir = (player.position - transform.position);
            if (!followPlayerY) dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f) dir.Normalize(); else dir = transform.forward;
        }
        forwardDir = dir;
        transform.rotation = Quaternion.LookRotation(forwardDir, Vector3.up);
        UpdateLifeUI();
    }

    void Awake()
    {
        if (remaining <= 0f) remaining = lifeSeconds;
        if (forwardDir.sqrMagnitude < 0.0001f) forwardDir = transform.forward.normalized;
        UpdateLifeUI();
    }

    void Update()
    {
        remaining -= Time.deltaTime;
        if (remaining <= 0f) { DestroySelf(); return; }
        UpdateLifeUI();

        if (player != null)
        {
            Vector3 to = (player.position - transform.position);
            if (!followPlayerY) to.y = 0f;
            if (to.sqrMagnitude > 0.0001f)
            {
                Vector3 desired = to.normalized;
                float maxRadians = Mathf.Deg2Rad * turnDegPerSec * Time.deltaTime;
                forwardDir = Vector3.RotateTowards(forwardDir, desired, maxRadians, 0f).normalized;

                if (followPlayerY)
                {
                    float targetY = player.position.y;
                    Vector3 pos = transform.position;
                    pos.y = Mathf.Lerp(pos.y, targetY, 1f - Mathf.Exp(-verticalFollowLerp * Time.deltaTime));
                    transform.position = pos;
                }
            }
        }

        transform.position += forwardDir * speed * Time.deltaTime;

        transform.rotation = Quaternion.LookRotation(forwardDir, Vector3.up);
    }

    private void UpdateLifeUI()
    {
        if (!lifeCircle) return;
        float fill = Mathf.Clamp01(remaining / Mathf.Max(0.0001f, lifeSeconds));
        lifeCircle.fillAmount = fill;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var health = other.GetComponent<PlayerHealth>();
        if (health != null) health.TakeDamage(damageOnTouch);

        var rb = other.attachedRigidbody;
        if (rb != null) rb.AddForce(forwardDir * pushImpulse, ForceMode.Impulse);

        foreach (var t in TurretInteractable.Instances)
        {
            if (t.IsPlayerInRange)
            {
                t.InterruptHold();
                break;
            }
        }

        DestroySelf();
    }

    private void OnDestroy()
    {
        spawner?.OnBotDestroyed(this);
    }

    private void DestroySelf()
    {
        spawner?.OnBotDestroyed(this);
        Destroy(gameObject);
    }

    public void KillByLaser()
    {
        DestroySelf();
    }

}
