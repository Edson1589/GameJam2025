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

    private bool attacking = false;

    private void Start()
    {
        if (!rb) rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (chargeHitboxObj != null)
        {
            chargeHitboxObj.SetActive(false);
        }

        StartCoroutine(BossLoop());
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
    }

    private IEnumerator BossLoop()
    {
        while (true)
        {
            if (!attacking)
            {
                attacking = true;

                int choice = Random.Range(0, 3);

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

    private IEnumerator DoSingleShockwaveJump()
    {
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

        if (chargeHitboxObj != null)
            chargeHitboxObj.SetActive(false);

        yield return new WaitForSeconds(chargeRecovery);
    }
}
