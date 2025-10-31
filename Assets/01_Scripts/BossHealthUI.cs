using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private Image fillImage;

    [Header("Visibilidad")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private bool hideWhenNoBoss = true;
    [SerializeField] private bool showOnStart = false;

    private void Awake()
    {
        if (!bossHealth)
            bossHealth = FindObjectOfType<BossHealth>();

        if (!group) group = GetComponent<CanvasGroup>();

        if (fillImage != null)
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        if (group != null)
            group.alpha = showOnStart ? 1f : 0f;

        RefreshUI();
    }

    private void Update()
    {
        if (!bossHealth)
        {
            if (hideWhenNoBoss && group) group.alpha = 0f;
            return;
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (!bossHealth) return;

        int cur = bossHealth.CurrentHP;
        int max = bossHealth.MaxHP;

        if (fillImage != null)
            fillImage.fillAmount = (max > 0) ? Mathf.Clamp01((float)cur / max) : 0f;

    }

    public void Show(bool on)
    {
        if (group) group.alpha = on ? 1f : 0f;
    }

    public void SetBoss(BossHealth newBoss)
    {
        bossHealth = newBoss;
        RefreshUI();
    }
}
