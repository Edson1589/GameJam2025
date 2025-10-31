using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Referencias base")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform bossVisual;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform[] muzzlePoints;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject shockwavePrefab;
    [SerializeField] private GameObject chargeHitboxObj;
    [SerializeField] private int maxConsecutiveSameAttack = 2;
    private int lastAttack = -1;
    private int sameAttackCount = 0;

    [Header("Arena / movimiento")]
    [SerializeField] private float arenaRadius = 12f;
    [SerializeField] private float bossHeight = 1.0f;
    [SerializeField] private float rotateSpeed = 10f;

    [Header("Ataque: Ondas de choque")]
    [SerializeField] private float jumpUpHeight = 2.5f;
    [SerializeField] private float jumpUpTime = 0.3f;
    [SerializeField] private float slamDownTime = 0.2f;

    [SerializeField] private int shockwaveJumpMinCount = 1;
    [SerializeField] private int shockwaveJumpMaxCount = 4;

    [SerializeField] private float delayBetweenShockwaveJumps = 0.4f;

    [SerializeField] private float shockwaveYOffset = 0.1f;

    [Header("Ataque: Ráfaga de disparos")]
    [SerializeField] private int burstsMin = 2;
    [SerializeField] private int burstsMax = 4;
    [SerializeField] private int bulletsPerBurstMin = 2;
    [SerializeField] private int bulletsPerBurstMax = 5;
    [SerializeField] private float burstInterval = 0.4f;
    [SerializeField] private float projectileSpeedMin = 10f;
    [SerializeField] private float projectileSpeedMax = 16f;
    [SerializeField] private Vector2 projectileScaleRange = new Vector2(0.6f, 1.4f);

    [Header("Ataque: Embestida")]
    [SerializeField] private float chargeWindupTime = 0.25f;
    [SerializeField] private float chargeSpeed = 20f;
    [SerializeField] private float chargeDuration = 0.4f;
    [SerializeField] private float chargeRecovery = 0.5f;

    [Header("Cadencia entre ataques")]
    [SerializeField] private float minAttackDelay = 0.5f;
    [SerializeField] private float maxAttackDelay = 1.2f;

    [Header("Activación")]
    [SerializeField] private bool startDormant = true;
    private bool isActive = false;
    private Coroutine loopCo;
    private bool attacking = false;

    [SerializeField] private Animator anim;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip sfxJump;
    [SerializeField] private AudioClip sfxShockwave;
    [SerializeField] private AudioClip sfxPunch;
    [SerializeField] private AudioClip sfxShoot;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.98f, 1.02f);
    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashDoJump = Animator.StringToHash("DoJump");
    private static readonly int HashDoShoot = Animator.StringToHash("DoShoot");
    private static readonly int HashDoCharge = Animator.StringToHash("DoCharge");
    private static readonly int HashActive = Animator.StringToHash("IsActive");


    private void Start()
    {
        if (!sfxSource)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 1f;
            sfxSource.dopplerLevel = 0.05f;
            sfxSource.rolloffMode = AudioRolloffMode.Linear;
            sfxSource.maxDistance = 35f;
        }

        if (!rb) rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (chargeHitboxObj != null)
            chargeHitboxObj.SetActive(true);

        if (!startDormant) Activate();
    }
    private void PlayOne(AudioClip clip, float volMul = 1f)
    {
        if (!clip || !sfxSource) return;
        sfxSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        sfxSource.PlayOneShot(clip, sfxVolume * volMul);
    }


    private void Update()
    {
        if (player != null)
        {
            Vector3 lookAt = player.position - transform.position;
            lookAt.y = 0f;
            if (lookAt.sqrMagnitude > 0.001f)
            {
                Quaternion offset = Quaternion.Euler(0, 90, 0);
                Quaternion targetRot = Quaternion.LookRotation(lookAt.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot * offset, rotateSpeed * Time.deltaTime);
            }
        }

        Vector3 p = transform.position;
        p.y = bossHeight;
        transform.position = p;

        Vector3 flat = new Vector3(transform.position.x, 0f, transform.position.z);
        if (flat.magnitude > arenaRadius)
        {
            flat = flat.normalized * arenaRadius;
            transform.position = new Vector3(flat.x, bossHeight, flat.z);
        }

        if (anim) anim.SetFloat(HashSpeed, 0f, 0.1f, Time.deltaTime);
    }

    public void Activate()
    {
        if (isActive) return;
        isActive = true;
        attacking = false;
        if (anim) anim.SetBool(HashActive, true);
        if (loopCo == null) loopCo = StartCoroutine(BossLoop());
    }

    public void Deactivate()
    {
        if (!isActive) return;
        isActive = false;
        attacking = false;
        if (anim) anim.SetBool(HashActive, false);
        if (loopCo != null) { StopCoroutine(loopCo); loopCo = null; }
    }

    private IEnumerator BossLoop()
    {
        while (isActive)
        {
            if (!attacking)
            {
                attacking = true;

                int choice = PickAttack();

                if (choice == 0)
                {
                    yield return StartCoroutine(DoShockwaveAttack());
                }
                else if (choice == 1)
                {
                    yield return StartCoroutine(DoShootingAttack());
                }
                else
                {
                    yield return StartCoroutine(DoChargeAttack());
                }

                attacking = false;
            }

            yield return new WaitForSeconds(Random.Range(minAttackDelay, maxAttackDelay));
        }
    }

    private int PickAttack()
    {
        int choice = Random.Range(0, 3);


        if (choice == lastAttack && sameAttackCount >= maxConsecutiveSameAttack)
        {
            int a = (lastAttack + 1) % 3;
            int b = (lastAttack + 2) % 3;
            choice = (Random.value < 0.5f) ? a : b;
        }

        if (choice == lastAttack) sameAttackCount++;
        else { lastAttack = choice; sameAttackCount = 1; }

        return choice;
    }

    private IEnumerator DoSingleShockwaveJump()
    {
        if (anim) anim.SetTrigger(HashDoJump);
        PlayOne(sfxJump);
        Vector3 startPos = transform.position;
        Vector3 apexPos = startPos + Vector3.up * jumpUpHeight;
        Vector3 landPos = startPos;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / jumpUpTime;
            transform.position = Vector3.Lerp(startPos, apexPos, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / slamDownTime;
            transform.position = Vector3.Lerp(apexPos, landPos, t);
            yield return null;
        }

        Vector3 spawnPos = transform.position + Vector3.up * shockwaveYOffset;
        if (sfxShockwave) AudioSource.PlayClipAtPoint(sfxShockwave, spawnPos, sfxVolume);
        SpawnShockwaveAt(spawnPos);

        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator DoShockwaveAttack()
    {
        int jumpsToDo = Random.Range(shockwaveJumpMinCount, shockwaveJumpMaxCount + 1);

        for (int j = 0; j < jumpsToDo; j++)
        {
            yield return StartCoroutine(DoSingleShockwaveJump());

            if (j < jumpsToDo - 1)
            {
                yield return new WaitForSeconds(delayBetweenShockwaveJumps);
            }
        }
        yield return new WaitForSeconds(0.25f);
    }


    private void SpawnShockwaveAt(Vector3 pos)
    {
        if (shockwavePrefab == null) return;

        GameObject waveObj = Instantiate(shockwavePrefab, pos, Quaternion.identity);

        BossShockwave ring = waveObj.GetComponent<BossShockwave>();
        if (ring != null)
        {
            ring.Init(player);
        }
    }


    private IEnumerator DoShootingAttack()
    {
        if (anim) anim.SetTrigger(HashDoShoot);
        PlayOne(sfxShoot);
        int bursts = Random.Range(burstsMin, burstsMax + 1);

        for (int b = 0; b < bursts; b++)
        {
            int bulletsThisBurst = Random.Range(bulletsPerBurstMin, bulletsPerBurstMax + 1);

            for (int i = 0; i < bulletsThisBurst; i++)
            {
                if (muzzlePoints.Length == 0) break;
                Transform muzzle = muzzlePoints[Random.Range(0, muzzlePoints.Length)];
                ShootOneProjectile(muzzle);
            }

            yield return new WaitForSeconds(burstInterval);
        }

        yield return new WaitForSeconds(0.2f);
    }

    private void ShootOneProjectile(Transform muzzle)
    {
        if (projectilePrefab == null || player == null) return;

        Vector3 dir = (player.position - muzzle.position).normalized;

        GameObject proj = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(dir, Vector3.up));

        float scaleFactor = Random.Range(projectileScaleRange.x, projectileScaleRange.y);
        proj.transform.localScale = Vector3.one * scaleFactor;

        float speed = Random.Range(projectileSpeedMin, projectileSpeedMax);

        BossProjectile bullet = proj.GetComponent<BossProjectile>();
        if (bullet != null)
        {
            bullet.Init(dir, speed);
        }
    }

    private IEnumerator DoChargeAttack()
    {
        if (player == null) yield break;
        if (anim) anim.SetTrigger(HashDoCharge);
        PlayOne(sfxPunch);

        Vector3 startPos = transform.position;
        Vector3 targetDirFlat = (player.position - startPos);
        targetDirFlat.y = 0f;
        targetDirFlat = targetDirFlat.normalized;

        yield return new WaitForSeconds(chargeWindupTime);

        if (chargeHitboxObj != null)
            chargeHitboxObj.SetActive(true);

        float timer = 0f;
        while (timer < chargeDuration)
        {
            timer += Time.deltaTime;

            Vector3 newPos = transform.position + targetDirFlat * chargeSpeed * Time.deltaTime;

            newPos.y = bossHeight;
            Vector3 flat = new Vector3(newPos.x, 0f, newPos.z);
            if (flat.magnitude > arenaRadius)
            {
                flat = flat.normalized * arenaRadius;
                newPos.x = flat.x;
                newPos.z = flat.z;
            }

            transform.position = newPos;

            yield return null;
        }

        yield return new WaitForSeconds(chargeRecovery);
    }

    public void ShowIdleVisual(bool on)
    {
        if (anim) anim.SetBool(HashActive, on);
    }

}
