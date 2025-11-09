using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private AnchorMother anchorMother; // Soporte para AnchorMother
    [SerializeField] private Image fillImage;

    [Header("Visibilidad")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private bool hideWhenNoBoss = true;
    [SerializeField] private bool showOnStart = false;

    private void Awake()
    {
        // Buscar BossHealth si no está asignado
        if (!bossHealth)
            bossHealth = FindObjectOfType<BossHealth>();

        // Buscar AnchorMother si no está asignado
        if (!anchorMother)
            anchorMother = FindObjectOfType<AnchorMother>();

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
        // Buscar nuevamente si se perdió la referencia
        if (!bossHealth && !anchorMother)
        {
            if (!bossHealth)
                bossHealth = FindObjectOfType<BossHealth>();
            if (!anchorMother)
                anchorMother = FindObjectOfType<AnchorMother>();

            if (hideWhenNoBoss && group) group.alpha = 0f;
            return;
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        float healthPercent = 0f;

        // Priorizar AnchorMother si existe
        if (anchorMother != null)
        {
            healthPercent = anchorMother.GetHealthPercent();
        }
        else if (bossHealth != null)
        {
            int cur = bossHealth.CurrentHP;
            int max = bossHealth.MaxHP;
            healthPercent = (max > 0) ? Mathf.Clamp01((float)cur / max) : 0f;
        }
        else
        {
            return; // No hay boss
        }

        if (fillImage != null)
            fillImage.fillAmount = healthPercent;

        // Mostrar la barra si hay un boss
        if (group != null && (bossHealth != null || anchorMother != null))
        {
            group.alpha = 1f;
        }
    }

    public void Show(bool on)
    {
        if (group) group.alpha = on ? 1f : 0f;
    }

    public void SetBoss(BossHealth newBoss)
    {
        bossHealth = newBoss;
        anchorMother = null; // Limpiar AnchorMother si se asigna BossHealth
        RefreshUI();
    }

    public void SetBoss(AnchorMother newBoss)
    {
        anchorMother = newBoss;
        bossHealth = null; // Limpiar BossHealth si se asigna AnchorMother
        RefreshUI();
    }
}
