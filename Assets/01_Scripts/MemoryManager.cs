using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MemoryManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject memoryPanel;
    [SerializeField] private TextMeshProUGUI memoryTitleText;
    [SerializeField] private TextMeshProUGUI memoryContentText;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private float displayDuration = 5f;

    [Header("Stats")]
    [SerializeField] private int totalMemories = 9;

    private float displayTimer = 0f;
    private bool isDisplaying = false;

    void Start()
    {
        if (memoryPanel != null)
        {
            memoryPanel.SetActive(false);
        }

        // Cargar memorias del GameManager al iniciar
        LoadMemoriesFromGameManager();
        UpdateCounter();
    }

    void Update()
    {
        if (isDisplaying)
        {
            displayTimer -= Time.deltaTime;
            if (displayTimer <= 0 || Input.GetKeyDown(KeyCode.E))
            {
                CloseMemoryPanel();
            }
        }
    }

    private void LoadMemoriesFromGameManager()
    {
        if (GameManager.Instance != null)
        {
            // Las memorias ya están en el GameManager, no necesitamos copiarlas
            Debug.Log($"Memorias cargadas del GameManager: {GameManager.Instance.GetMemoryCount()}");
        }
    }

    public void CollectMemory(int memoryID, string memoryText)
    {
        // Verificar si ya fue colectada (desde GameManager)
        if (GameManager.Instance != null && GameManager.Instance.HasMemory(memoryID))
        {
            Debug.LogWarning($"Eco-Memoria #{memoryID} ya fue colectada");
            return;
        }

        // Guardar en GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectMemory(memoryID);
        }

        Debug.Log($"✓ Eco-Memoria #{memoryID} colectada ({GetCollectedCount()}/{totalMemories})");

        // Mostrar en UI
        ShowMemoryPanel(memoryID, memoryText);

        // Actualizar contador
        UpdateCounter();
    }

    private void ShowMemoryPanel(int memoryID, string memoryText)
    {
        if (memoryPanel == null) return;

        memoryPanel.SetActive(true);
        isDisplaying = true;
        displayTimer = displayDuration;

        if (memoryTitleText != null)
        {
            memoryTitleText.text = $"ECO-MEMORIE #{memoryID}";
        }

        if (memoryContentText != null)
        {
            memoryContentText.text = memoryText;
        }
    }

    private void CloseMemoryPanel()
    {
        if (memoryPanel != null)
        {
            memoryPanel.SetActive(false);
        }
        isDisplaying = false;
    }

    private void UpdateCounter()
    {
        if (counterText != null)
        {
            int collected = GetCollectedCount();
            counterText.text = $"Memories: {collected}/{totalMemories}";
        }
    }

    // Métodos públicos para consultar progreso (ahora desde GameManager)
    public int GetCollectedCount()
    {
        return GameManager.Instance != null ? GameManager.Instance.GetMemoryCount() : 0;
    }

    public int GetTotalCount() => totalMemories;

    public bool HasCollectedAll()
    {
        return GetCollectedCount() >= totalMemories;
    }
}