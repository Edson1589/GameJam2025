using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class TurretInteractable : MonoBehaviour
{
    public static readonly HashSet<TurretInteractable> Instances = new HashSet<TurretInteractable>();

    [Header("Interacción")]
    [SerializeField] private float holdSeconds = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    [SerializeField] private CanvasGroup uiGroup;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image circleImage;

    [Header("Boss")]
    [SerializeField] private BossHealth bossHealth;

    private bool playerInRange = false;
    private bool isDisabled = false;
    public float progress = 0f;
    private Collider triggerCol;

    private bool requireKeyUpBeforeContinue = false;

    public bool IsPlayerInRange => playerInRange;

    void OnEnable() { Instances.Add(this); }
    void OnDisable() { Instances.Remove(this); }
    void Awake()
    {
        triggerCol = GetComponent<Collider>();
        triggerCol.isTrigger = true;

        if (uiGroup) uiGroup.alpha = 0f;
        if (promptText) promptText.text = "Mantén presionado E";
        if (circleImage)
        {
            circleImage.type = Image.Type.Filled;
            circleImage.fillMethod = Image.FillMethod.Radial360;
            circleImage.fillAmount = 0f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDisabled) return;
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        ShowUI();
        UpdateCircle();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        HideUI();
    }

    void Update()
    {
        if (isDisabled || !playerInRange) return;

        if (requireKeyUpBeforeContinue)
        {
            if (!Input.GetKey(interactKey))
                requireKeyUpBeforeContinue = false;
            return;
        }

        if (Input.GetKey(interactKey))
        {
            progress += Time.deltaTime;
            UpdateCircle();

            if (progress >= holdSeconds)
                CompleteAndDisable();
        }
    }

    private void UpdateCircle()
    {
        if (!circleImage) return;
        float fill = Mathf.Clamp01(progress / holdSeconds);
        circleImage.fillAmount = fill;
    }

    private void ShowUI()
    {
        if (uiGroup) uiGroup.alpha = 1f;
    }

    private void HideUI()
    {
        if (uiGroup) uiGroup.alpha = 0f;
    }

    private void CompleteAndDisable()
    {
        isDisabled = true;
        progress = holdSeconds;
        UpdateCircle();
        HideUI();

        if (triggerCol) triggerCol.enabled = false;

        if (bossHealth) bossHealth.ApplyTurretDamage();
    }

    public void InterruptHold()
    {
        if (isDisabled) return;
        requireKeyUpBeforeContinue = true;
    }
}
