using UnityEngine;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    [Header("Vida del Jefe")]
    [SerializeField] public int maxHP = 100;
    [SerializeField] public int currentHP;

    [Header("Daño por torreta desactivada")]
    [SerializeField] private int damagePerTurret = 25;

    [Header("Referencias")]
    [SerializeField] private BossController boss;

    private bool isDead = false;

    [Header("Activar al morir")]
    [SerializeField] private GameObject[] enableOnDeath;

    [SerializeField] private Collider[] enableCollidersOnDeath;

    [SerializeField] private float showTriggerDelay = 0f;

    void Awake()
    {
        currentHP = maxHP;
        if (!boss) boss = GetComponent<BossController>();
    }

    public void ApplyTurretDamage()
    {
        ApplyDamage(damagePerTurret);
    }

    public void ApplyDamage(int amount)
    {
        if (isDead || amount <= 0) return;

        currentHP = Mathf.Max(0, currentHP - amount);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (boss) boss.OnBossDeathRequested();
        else Destroy(gameObject);

        if (showTriggerDelay > 0f)
            StartCoroutine(EnableTargetsAfterDelay(showTriggerDelay));
        else
            EnableTargets();
    }

    private IEnumerator EnableTargetsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnableTargets();
    }

    private void EnableTargets()
    {
        if (enableOnDeath != null)
        {
            foreach (var go in enableOnDeath)
                if (go) go.SetActive(true);
        }

        if (enableCollidersOnDeath != null)
        {
            foreach (var col in enableCollidersOnDeath)
                if (col) col.enabled = true;
        }
    }

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
}
