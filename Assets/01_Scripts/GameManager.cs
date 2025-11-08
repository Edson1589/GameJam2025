using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager - Mantiene el progreso del jugador entre escenas
/// Singleton que persiste con DontDestroyOnLoad
/// </summary>
public class GameManager : MonoBehaviour
{
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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // No destruir al cambiar de escena

            if (showDebugLogs)
                Debug.Log("═══ GameManager creado - Progreso persistente activado ═══");

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


    /// <summary>
    /// Resetea el progreso de las partes del RUBO (hasLegs, hasArms, hasTorso)
    /// a 'false' justo antes de recargar la escena de un nivel.
    /// </summary>
    public void ResetPartsForRestart(string sceneName)
    {
        if (sceneName.Contains("Level_01") || sceneName.Contains("Ensamble"))
        {
            hasLegs = false;
        }
        else if (sceneName.Contains("Level_02") || sceneName.Contains("Tapes"))
        {
            hasArms = false;
        }
        else if (sceneName.Contains("Level_03") || sceneName.Contains("Azotea"))
        {
            hasTorso = false;
        }
        else
        {
            // Para niveles no definidos o test, resetea todas.
            hasLegs = false;
            hasArms = false;
            hasTorso = false;
        }

        if (showDebugLogs)
            Debug.Log($">>> GameManager: Progreso de Partes reseteado para {sceneName}.");
    }

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

    /// <summary>
    /// Aplica el progreso persistente al jugador. 
    /// Delega la lógica de activación visual/física al PlayerController.
    /// </summary>
    public void ApplyProgressToPlayer(PlayerController player)
    {
        if (player == null) return;

        bool currentHasLegs = hasLegs;
        bool currentHasArms = hasArms;
        bool currentHasTorso = hasTorso;
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Nivel 2: Forzar sin piernas
        if (currentSceneName.Contains("Level_02") || currentSceneName.Contains("Tapes"))
        {
            currentHasLegs = false;
            if (showDebugLogs)
                Debug.Log($"SOBREESCRITURA: Iniciando Nivel 2 ({currentSceneName}). Forzando hasLegs = FALSE.");
        }
        // Nivel 3: Forzar todas las partes (delegar a Level3Manager si existe)
        else if (Level3Manager.IsLevel3Scene(currentSceneName))
        {
            // Si Level3Manager está activo, dejar que él maneje la inicialización
            if (Level3Manager.Instance != null && Level3Manager.Instance.IsActive())
            {
                if (showDebugLogs)
                    Debug.Log($"Nivel 3 detectado - Level3Manager se encargará de la inicialización");
                // Level3Manager se encargará de asegurar que el jugador tenga todas las partes
                currentHasLegs = true;
                currentHasArms = true;
                currentHasTorso = true;
            }
            else
            {
                // Fallback: forzar todas las partes si Level3Manager no está disponible
                currentHasLegs = true;
                currentHasArms = true;
                currentHasTorso = true;
                if (showDebugLogs)
                    Debug.Log($"Nivel 3 detectado - Forzando todas las partes (Level3Manager no disponible)");
            }
        }

        player.ApplyProgressFromManager(currentHasLegs, currentHasArms, currentHasTorso);

        if (showDebugLogs)
        {
            Debug.Log($"═══ Progreso aplicado al jugador ═══");
            Debug.Log($"Piernas: {currentHasLegs} | Brazos: {currentHasArms} | Torso: {currentHasTorso}");
        }
    }

    // Helpers
    private bool IsMainMenuScene(string sceneName)
    {
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