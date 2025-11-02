using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Vida del Jefe")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP;

    [Header("Daño por torreta desactivada")]
    [SerializeField] private int damagePerTurret = 25;

    [Header("Referencias")]
    [SerializeField] private BossController boss;

    private bool isDead = false;

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
    }


    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
}
