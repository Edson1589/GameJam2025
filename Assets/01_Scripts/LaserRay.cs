using UnityEngine;

public class LaserRay : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform firePoint;

    [Header("Proyectil")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 40f;
    [SerializeField] private float projectileLifetime = 3f;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 0.25f;
    private float nextFireTime = 0f;

    [Header("Desbloqueo")]
    [SerializeField] private bool requiresTorso = true;
    [SerializeField] private bool unlocked = false;

    [Header("SFX / VFX (opcionales)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip sfxShoot;
    [SerializeField] private GameObject muzzleVFX;
    [SerializeField] private float muzzleVFXLife = 0.5f;

    [SerializeField] private Transform camTransform;

    private void Awake()
    {
        if (!sfxSource) sfxSource = GetComponent<AudioSource>();
        if (!camTransform && Camera.main) camTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryFire();
    }

    public void SetUnlocked(bool on) => unlocked = on;
    public bool IsUnlocked => unlocked;
    public bool IsReady() => Time.time >= nextFireTime;

    public float Cooldown01()
    {
        if (cooldown <= 0f) return 1f;
        float remaining = nextFireTime - Time.time;
        return Mathf.Clamp01(1f - Mathf.Clamp01(remaining / cooldown));
    }

    public void TryFire()
    {
        if (!IsReady()) return;
        if (requiresTorso && !unlocked) return;
        if (!projectilePrefab || !firePoint) return;

        Fire();
        nextFireTime = Time.time + cooldown;
    }

    private void Fire()
    {
        Vector3 origin = firePoint.position;
        Vector3 dir = (camTransform ? camTransform.forward : firePoint.forward);

        GameObject go = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(dir, Vector3.up));

        var bullet = go.GetComponent<BulletProjectile>();
        if (bullet != null)
        {
            bullet.Init(transform.root, dir, projectileSpeed, projectileLifetime);
        }
        else
        {
            var rb = go.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.velocity = dir.normalized * projectileSpeed;
                Destroy(go, projectileLifetime);
            }
        }

        if (muzzleVFX)
        {
            var fx = Instantiate(muzzleVFX, firePoint.position, firePoint.rotation, firePoint);
            Destroy(fx, muzzleVFXLife);
        }
        if (sfxSource && sfxShoot) sfxSource.PlayOneShot(sfxShoot);
    }

}
