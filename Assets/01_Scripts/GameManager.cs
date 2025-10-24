using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager - Mantiene el progreso del jugador entre escenas
/// Singleton que persiste con DontDestroyOnLoad
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    [Header("Player Progress")]
    public bool hasLegs = false;
    public bool hasArms = false;
    public bool hasTorso = false;
    public HashSet<int> collectedMemories = new HashSet<int>();

    [Header("Scene Management")]
    [SerializeField] private string[] testLevelNames = { "Level_Test", "TestLevel", "Test" };

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool resetProgressOnMainMenu = true;

    void Awake()
    {
        // Implementar Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // No destruir al cambiar de escena

            if (showDebugLogs)
                Debug.Log("═══ GameManager creado - Progreso persistente activado ═══");

            // Suscribirse a eventos de carga de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject); // Si ya existe otro, destruir este
            return;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void Update()
    {
        // Presiona P para ver el progreso actual
        if (showDebugLogs && Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"═══ PROGRESO ACTUAL ═══\n{GetProgressString()}");
        }
    }

    // Evento cuando se carga una escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (showDebugLogs)
            Debug.Log($"═══ Escena cargada: {scene.name} ═══");

        // SOLO resetear en MainMenu, NO en niveles del juego
        if (resetProgressOnMainMenu && IsMainMenuScene(scene.name))
        {
            ResetProgress();
            if (showDebugLogs)
                Debug.Log("MainMenu detectado - Progreso reseteado");
        }
        else if (IsTestLevel(scene.name))
        {
            ResetProgress();
            if (showDebugLogs)
                Debug.Log("Nivel de PRUEBA detectado - Progreso reseteado para testing");
        }
        else
        {
            // Es un nivel normal, MANTENER el progreso
            if (showDebugLogs)
                Debug.Log($"Nivel normal: {scene.name} - Manteniendo progreso: {GetProgressString()}");
        }

        // Buscar al jugador y aplicar progreso
        StartCoroutine(ApplyProgressToPlayerDelayed());
    }

    private System.Collections.IEnumerator ApplyProgressToPlayerDelayed()
    {
        // Esperar un frame para que el jugador se inicialice
        yield return null;

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            PlayerController player = playerGO.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyProgressToPlayer(player);
            }
        }
    }

    // Métodos para actualizar progreso
    public void CollectLegs()
    {
        hasLegs = true;
        if (showDebugLogs)
            Debug.Log(">>> GameManager: Piernas GUARDADAS (Persistente entre escenas)");
    }

    public void CollectArms()
    {
        hasArms = true;
        if (showDebugLogs)
            Debug.Log(">>> GameManager: Brazos GUARDADOS (Persistente entre escenas)");
    }

    public void CollectTorso()
    {
        hasTorso = true;
        if (showDebugLogs)
            Debug.Log(">>> GameManager: Torso GUARDADO (Persistente entre escenas)");
    }

    public void CollectMemory(int memoryID)
    {
        if (!collectedMemories.Contains(memoryID))
        {
            collectedMemories.Add(memoryID);
            if (showDebugLogs)
                Debug.Log($">>> GameManager: Memoria #{memoryID} GUARDADA. Total: {collectedMemories.Count}");
        }
    }

    public bool HasMemory(int memoryID)
    {
        return collectedMemories.Contains(memoryID);
    }

    public int GetMemoryCount()
    {
        return collectedMemories.Count;
    }

    public HashSet<int> GetCollectedMemories()
    {
        return new HashSet<int>(collectedMemories); // Retornar copia
    }

    // Resetear progreso COMPLETO
    public void ResetProgress()
    {
        hasLegs = false;
        hasArms = false;
        hasTorso = false;
        collectedMemories.Clear();

        if (showDebugLogs)
            Debug.Log(">>> GameManager: Progreso RESETEADO");
    }

    // NUEVO: Resetear solo las memorias del nivel actual
    public void ResetMemoriesForLevel(string sceneName)
    {
        HashSet<int> memoriesToRemove = new HashSet<int>();

        // Determinar qué memorias pertenecen a este nivel
        if (sceneName.Contains("Level_01") || sceneName.Contains("Ensamble"))
        {
            // Nivel 1: Memorias 1, 2, 3
            memoriesToRemove.Add(1);
            memoriesToRemove.Add(2);
            memoriesToRemove.Add(3);
        }
        else if (sceneName.Contains("Level_02") || sceneName.Contains("Tapes"))
        {
            // Nivel 2: Memorias 4, 5, 6
            memoriesToRemove.Add(4);
            memoriesToRemove.Add(5);
            memoriesToRemove.Add(6);
        }
        else if (sceneName.Contains("Level_03") || sceneName.Contains("Azotea"))
        {
            // Nivel 3: Memorias 7, 8, 9
            memoriesToRemove.Add(7);
            memoriesToRemove.Add(8);
            memoriesToRemove.Add(9);
        }
        else if (IsTestLevel(sceneName))
        {
            // En niveles de test, resetear todas
            collectedMemories.Clear();
            if (showDebugLogs)
                Debug.Log(">>> GameManager: TODAS las memorias reseteadas (Test Level)");
            return;
        }

        // Remover las memorias del nivel actual
        foreach (int memoryID in memoriesToRemove)
        {
            if (collectedMemories.Contains(memoryID))
            {
                collectedMemories.Remove(memoryID);
            }
        }

        if (showDebugLogs && memoriesToRemove.Count > 0)
            Debug.Log($">>> GameManager: Memorias del nivel {sceneName} reseteadas. Quedan: {collectedMemories.Count}");
    }

    // Verificar si está completo
    public bool IsFullyAssembled()
    {
        return hasLegs && hasArms && hasTorso;
    }

    public void ApplyProgressToPlayer(PlayerController player)
    {
        if (player == null) return;

        bool anyPartRestored = false;

        if (hasLegs && !player.hasLegs)
        {
            player.hasLegs = true;
            if (player.legsGroup != null)
                player.legsGroup.SetActive(true);
            anyPartRestored = true;
        }

        if (hasArms && !player.hasArms)
        {
            player.hasArms = true;
            if (player.armsGroup != null)
                player.armsGroup.SetActive(true);
            anyPartRestored = true;
        }

        if (hasTorso && !player.hasTorso)
        {
            player.hasTorso = true;
            if (player.torsoGroup != null)
                player.torsoGroup.SetActive(true);
            anyPartRestored = true;
        }

        if (anyPartRestored)
        {
            player.SendMessage("UpdateStatusText", SendMessageOptions.DontRequireReceiver);
            player.SendMessage("UpdateInstructions", SendMessageOptions.DontRequireReceiver);

            if (showDebugLogs)
            {
                Debug.Log($"═══ Progreso aplicado al jugador ═══");
                Debug.Log($"Piernas: {hasLegs} | Brazos: {hasArms} | Torso: {hasTorso}");
            }
        }
    }

    // Helpers
    private bool IsMainMenuScene(string sceneName)
    {
        // Solo considerar MainMenu si es EXACTAMENTE el menú principal
        // NO detectar escenas que contengan "Menu" en su nombre
        return sceneName.Equals("MainMenu", System.StringComparison.OrdinalIgnoreCase)
               || sceneName.Equals("Menu", System.StringComparison.OrdinalIgnoreCase)
               || sceneName.Equals("StartMenu", System.StringComparison.OrdinalIgnoreCase);
    }

    private bool IsTestLevel(string sceneName)
    {
        foreach (string testName in testLevelNames)
        {
            if (sceneName.Contains(testName))
                return true;
        }
        return false;
    }

    // Método para UI/Debug
    public string GetProgressString()
    {
        return $"Piernas: {(hasLegs ? "✓" : "✗")} | Brazos: {(hasArms ? "✓" : "✗")} | Torso: {(hasTorso ? "✓" : "✗")} | Memorias: {collectedMemories.Count}";
    }
}