using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BulletProjectile : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed = 40f;

    [Header("Vida")]
    [SerializeField] private float lifeTime = 3f;

    [Header("Colisión")]
    [SerializeField] private bool destroyOnAnyNonTrigger = true;

    private Vector3 velocity;
    private Transform ownerRoot;
    private float life;

    public void Init(Transform ownerRoot, Vector3 dir, float spd, float lifeSeconds)
    {
        this.ownerRoot = ownerRoot;
        this.velocity = dir.normalized * spd;
        this.speed = spd;
        this.life = lifeSeconds;
    }

    private void Awake()
    {
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        if (life <= 0f) life = lifeTime;
    }

    private void Update()
    {
        transform.position += velocity * Time.deltaTime;

        life -= Time.deltaTime;
        if (life <= 0f) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ownerRoot && other.transform.root == ownerRoot) return;

        // Detectar jefe AnchorMother
        var anchorMother = other.GetComponent<AnchorMother>() ?? other.GetComponentInParent<AnchorMother>();
        if (anchorMother != null)
        {
            anchorMother.TakeDamage(10); // Daño por bala (puede ser configurado en AnchorMother)
            Destroy(gameObject);
            return;
        }

        var bot = other.GetComponent<PusherBot>() ?? other.GetComponentInParent<PusherBot>();
        if (bot != null)
        {
            bot.KillByLaser();
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger && destroyOnAnyNonTrigger)
        {
            Destroy(gameObject);
        }
    }
}
