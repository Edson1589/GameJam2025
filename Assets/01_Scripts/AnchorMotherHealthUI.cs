using UnityEngine;
using UnityEngine.UI;

public class AnchorMotherHealthUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AnchorMother boss;
    [SerializeField] private Image fillImage; // El objeto "Relleno" dentro de BossHealthBar

    [Header("Visibilidad")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool hideWhenNoBoss = true;
    [SerializeField] private bool showOnStart = false;

    private void Awake()
    {
        // Buscar el boss si no está asignado
        if (boss == null)
        {
            boss = FindObjectOfType<AnchorMother>();
        }

        // Buscar CanvasGroup si no está asignado
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        // Configurar la imagen de relleno
        if (fillImage != null)
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        // Configurar visibilidad inicial
        if (canvasGroup != null)
        {
            canvasGroup.alpha = showOnStart ? 1f : 0f;
        }

        RefreshUI();
    }

    private void Update()
    {
        // Buscar el boss si se perdió la referencia
        if (boss == null)
        {
            boss = FindObjectOfType<AnchorMother>();
            if (boss == null && hideWhenNoBoss && canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                return;
            }
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (boss == null) return;

        // Obtener la salud del boss usando reflexión o métodos públicos
        // Como AnchorMother tiene campos privados, necesitamos acceder de otra forma
        // Por ahora, usaremos un método público que debemos agregar a AnchorMother
        
        if (fillImage != null)
        {
            float healthPercent = boss.GetHealthPercent();
            fillImage.fillAmount = Mathf.Clamp01(healthPercent);
        }

        // Mostrar la barra si el boss existe
        if (canvasGroup != null && boss != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    public void Show(bool on)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = on ? 1f : 0f;
        }
    }

    public void SetBoss(AnchorMother newBoss)
    {
        boss = newBoss;
        RefreshUI();
    }
}

