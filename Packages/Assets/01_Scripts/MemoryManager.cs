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

    private HashSet<int> collectedMemories = new HashSet<int>();
    private float displayTimer = 0f;
    private bool isDisplaying = false;

    void Start()
    {
        if (memoryPanel != null)
        {
            memoryPanel.SetActive(false);
        }

        UpdateCounter();
    }

    void Update()
    {
        // Cerrar panel automáticamente después del tiempo
        if (isDisplaying)
        {
            displayTimer -= Time.deltaTime;

            if (displayTimer <= 0 || Input.GetKeyDown(KeyCode.E))
            {
                CloseMemoryPanel();
            }
        }
    }

    public void CollectMemory(int memoryID, string memoryText)
    {
        // Evitar recolectar la misma memoria dos veces
        if (collectedMemories.Contains(memoryID))
        {
            Debug.LogWarning($"Eco-Memoria #{memoryID} ya fue colectada");
            return;
        }

        // Registrar memoria
        collectedMemories.Add(memoryID);

        Debug.Log($"? Eco-Memoria #{memoryID} colectada ({collectedMemories.Count}/{totalMemories})");

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
            counterText.text = $"Memories: {collectedMemories.Count}/{totalMemories}";
        }
    }

    // Métodos públicos para consultar progreso
    public int GetCollectedCount() => collectedMemories.Count;
    public int GetTotalCount() => totalMemories;
    public bool HasCollectedAll() => collectedMemories.Count >= totalMemories;
}