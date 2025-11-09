using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Level3Manager - Gestiona toda la lógica específica del Nivel 3 (Azotea)
/// Este script centraliza las mecánicas del nivel 3 para que los cambios no afecten otros niveles
/// </summary>
public class Level3Manager : MonoBehaviour
{
    public static Level3Manager Instance { get; private set; }

    [Header("Level 3 Configuration")]
    [SerializeField] private bool autoInitializeOnStart = true;
    [SerializeField] private bool ensurePlayerHasAllParts = true;

    [Header("Boss Reference")]
    [SerializeField] private AnchorMother bossAnchorMother;

    [Header("Player Setup")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Level 3 Specific Settings")]
    [SerializeField] private bool overridePlayerSpeed = false;
    [SerializeField] private float level3MoveSpeed = 30f;
    [SerializeField] private int level3MaxJumps = 2;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private bool enablePauseMenu = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private bool isLevel3Active = false;
    private bool isPaused = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Verificar que estamos en el nivel 3
        string currentScene = SceneManager.GetActiveScene().name;
        isLevel3Active = IsLevel3Scene(currentScene);

        if (!isLevel3Active)
        {
            if (showDebugLogs)
                Debug.LogWarning($"Level3Manager detectado en escena que NO es Nivel 3: {currentScene}. Desactivando...");
            enabled = false;
            return;
        }

        if (showDebugLogs)
            Debug.Log("═══ Level3Manager activado - Nivel 3 (Azotea) ═══");
    }

    void Start()
    {
        if (!isLevel3Active) return;

        if (autoInitializeOnStart)
        {
            InitializeLevel3();
            // Ejecutar también con un pequeño delay para asegurar que todo se muestre
            StartCoroutine(EnsurePartsVisibleDelayed());
        }
    }

    void Update()
    {
        if (!isLevel3Active) return;

        // Manejar pausa con ESC
        if (enablePauseMenu && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Coroutine que asegura que las partes estén visibles después de un pequeño delay
    /// Útil para casos donde la inicialización necesita esperar a que Unity termine de cargar
    /// </summary>
    private System.Collections.IEnumerator EnsurePartsVisibleDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        
        if (playerController == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                playerController = playerGO.GetComponent<PlayerController>();
            }
        }

        if (playerController != null)
        {
            // Forzar activación visual de todas las partes
            if (playerController.legsGroup != null) playerController.legsGroup.SetActive(true);
            if (playerController.armsGroup != null) playerController.armsGroup.SetActive(true);
            if (playerController.torsoGroup != null) playerController.torsoGroup.SetActive(true);

            // Asegurar que los flags estén correctos
            playerController.hasLegs = true;
            playerController.hasArms = true;
            playerController.hasTorso = true;

            // Desbloquear el laser ya que tiene torso
            LaserRay laser = playerController.GetComponent<LaserRay>();
            if (laser == null)
            {
                laser = playerController.GetComponentInChildren<LaserRay>();
            }
            if (laser != null)
            {
                laser.SetUnlocked(true);
                if (showDebugLogs)
                    Debug.Log(">>> Level3Manager: Laser desbloqueado (jugador tiene torso)");
            }
            else
            {
                Debug.LogWarning(">>> Level3Manager: No se encontró componente LaserRay en el jugador");
            }

            // Actualizar UI
            playerController.UpdateStatusText();
            playerController.UpdateInstructions();

            if (showDebugLogs)
                Debug.Log(">>> Level3Manager: Verificación tardía - Todas las partes están visibles");
        }
    }

    /// <summary>
    /// Inicializa todas las mecánicas específicas del nivel 3
    /// </summary>
    public void InitializeLevel3()
    {
        if (!isLevel3Active)
        {
            Debug.LogWarning("Level3Manager: Intento de inicializar fuera del Nivel 3");
            return;
        }

        if (showDebugLogs)
            Debug.Log(">>> Inicializando Nivel 3...");

        // Asegurar que el jugador tiene todas las partes
        if (ensurePlayerHasAllParts)
        {
            EnsurePlayerHasAllParts();
        }

        // Aplicar configuraciones específicas del nivel 3
        ApplyLevel3Settings();

        // Inicializar el jefe si existe
        if (bossAnchorMother != null)
        {
            if (showDebugLogs)
                Debug.Log(">>> Jefe AnchorMother encontrado y listo");
        }
        else
        {
            Debug.LogWarning("Level3Manager: No se encontró referencia al AnchorMother");
        }

        if (showDebugLogs)
            Debug.Log(">>> Nivel 3 inicializado correctamente");
    }

    /// <summary>
    /// Asegura que el jugador tenga todas las partes del cuerpo en el nivel 3
    /// Y que se muestren visualmente todas las piezas del robot
    /// </summary>
    private void EnsurePlayerHasAllParts()
    {
        // Buscar jugador si no está asignado
        if (playerController == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                playerController = playerGO.GetComponent<PlayerController>();
            }
        }

        if (playerController == null)
        {
            Debug.LogError("Level3Manager: No se encontró PlayerController");
            return;
        }

        // Forzar que todas las partes estén activas VISUALMENTE
        // Primero activamos los grupos visuales directamente
        if (playerController.legsGroup != null)
        {
            playerController.legsGroup.SetActive(true);
            if (showDebugLogs)
                Debug.Log(">>> Level3Manager: Piernas activadas visualmente");
        }

        if (playerController.armsGroup != null)
        {
            playerController.armsGroup.SetActive(true);
            if (showDebugLogs)
                Debug.Log(">>> Level3Manager: Brazos activados visualmente");
        }

        if (playerController.torsoGroup != null)
        {
            playerController.torsoGroup.SetActive(true);
            if (showDebugLogs)
                Debug.Log(">>> Level3Manager: Torso activado visualmente");
        }

        // Luego conectamos las partes usando los métodos del PlayerController
        // Esto actualiza la lógica, colliders, y posiciones
        if (!playerController.hasLegs)
        {
            playerController.ConnectLegs();
            if (showDebugLogs)
                Debug.Log(">>> Level3Manager: Piernas conectadas al jugador");
        }
        else
        {
            // Si ya tiene piernas pero no están visibles, forzar activación
            if (playerController.legsGroup != null && !playerController.legsGroup.activeSelf)
            {
                playerController.legsGroup.SetActive(true);
            }
        }

        if (!playerController.hasArms)
        {
            playerController.ConnectArms();
            if (showDebugLogs)
                Debug.Log(">>> Level3Manager: Brazos conectados al jugador");
        }
        else
        {
            // Si ya tiene brazos pero no están visibles, forzar activación
            if (playerController.armsGroup != null && !playerController.armsGroup.activeSelf)
            {
                playerController.armsGroup.SetActive(true);
            }
        }

        if (!playerController.hasTorso)
        {
            playerController.ConnectTorso();
            if (showDebugLogs)
                Debug.Log(">>> Level3Manager: Torso conectado al jugador");
        }
        else
        {
            // Si ya tiene torso pero no está visible, forzar activación
            if (playerController.torsoGroup != null && !playerController.torsoGroup.activeSelf)
            {
                playerController.torsoGroup.SetActive(true);
            }
        }

        // Asegurar que todas las partes estén activas una vez más (por si acaso)
        if (playerController.legsGroup != null) playerController.legsGroup.SetActive(true);
        if (playerController.armsGroup != null) playerController.armsGroup.SetActive(true);
        if (playerController.torsoGroup != null) playerController.torsoGroup.SetActive(true);

        // Actualizar salud si existe PlayerHealth
        if (playerHealth == null)
        {
            playerHealth = playerController.GetComponent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            // Inicializar con todas las partes (legs, arms, torso)
            playerHealth.InitializeFromParts(true, true, true);
            
            // Verificar que se haya actualizado correctamente
            int newMaxHP = playerHealth.GetOwnedMax();
            int expectedMax = 40 + 25 + 20 + 35; // head + legs + arms + torso = 120
            if (newMaxHP == expectedMax)
            {
                if (showDebugLogs)
                    Debug.Log($">>> Level3Manager: Salud inicializada correctamente: {newMaxHP} HP (todas las partes)");
            }
            else
            {
                Debug.LogWarning($">>> Level3Manager: Salud no se inicializó correctamente. Esperado: {expectedMax}, Obtenido: {newMaxHP}");
            }
        }

        // Actualizar GameManager para mantener consistencia
        if (GameManager.Instance != null)
        {
            GameManager.Instance.hasLegs = true;
            GameManager.Instance.hasArms = true;
            GameManager.Instance.hasTorso = true;
        }

        // Permitir salto sin piernas en nivel 3
        playerController.SetAllowJumpWithoutLegs(true);
        if (showDebugLogs)
            Debug.Log(">>> Level3Manager: Salto habilitado sin necesidad de piernas");

        // Desbloquear el laser ya que tiene torso
        LaserRay laser = playerController.GetComponent<LaserRay>();
        if (laser == null)
        {
            laser = playerController.GetComponentInChildren<LaserRay>();
        }
        if (laser != null)
        {
            laser.SetUnlocked(true);
            if (showDebugLogs)
                Debug.Log(">>> Level3Manager: Laser desbloqueado (jugador tiene torso)");
        }
        else
        {
            Debug.LogWarning(">>> Level3Manager: No se encontró componente LaserRay en el jugador");
        }

        // Forzar actualización del estado visual y UI
        playerController.UpdateStatusText();
        playerController.UpdateInstructions();

        if (showDebugLogs)
        {
            Debug.Log(">>> Level3Manager: Verificación final de partes visibles:");
            Debug.Log($"    - Piernas: {(playerController.legsGroup != null && playerController.legsGroup.activeSelf ? "VISIBLE" : "OCULTA")}");
            Debug.Log($"    - Brazos: {(playerController.armsGroup != null && playerController.armsGroup.activeSelf ? "VISIBLE" : "OCULTA")}");
            Debug.Log($"    - Torso: {(playerController.torsoGroup != null && playerController.torsoGroup.activeSelf ? "VISIBLE" : "OCULTA")}");
        }
    }

    /// <summary>
    /// Aplica configuraciones específicas del nivel 3 al jugador
    /// </summary>
    private void ApplyLevel3Settings()
    {
        if (playerController == null) return;

        // Aquí puedes agregar cualquier configuración específica del nivel 3
        // Por ejemplo, velocidad, saltos, etc.
        
        if (overridePlayerSpeed)
        {
            // Nota: Esto requeriría que moveSpeed sea público o tener un método setter
            // Por ahora solo lo documentamos
            if (showDebugLogs)
                Debug.Log($">>> Level3Manager: Configuración de velocidad aplicada (requiere implementación en PlayerController)");
        }
    }

    /// <summary>
    /// Verifica si la escena actual es el Nivel 3
    /// </summary>
    public static bool IsLevel3Scene(string sceneName)
    {
        return sceneName.Contains("Level_03") || 
               sceneName.Contains("Azotea") ||
               sceneName.Contains("Level3");
    }

    /// <summary>
    /// Verifica si estamos actualmente en el Nivel 3
    /// </summary>
    public bool IsActive()
    {
        return isLevel3Active;
    }

    /// <summary>
    /// Obtiene la referencia al jefe del nivel 3
    /// </summary>
    public AnchorMother GetBoss()
    {
        return bossAnchorMother;
    }

    /// <summary>
    /// Método para resetear el nivel 3 (útil para reiniciar)
    /// </summary>
    public void ResetLevel3()
    {
        if (!isLevel3Active) return;

        if (showDebugLogs)
            Debug.Log(">>> Level3Manager: Reseteando Nivel 3...");

        // Aquí puedes agregar lógica de reset específica
        // Por ejemplo, resetear paneles del jefe, etc.

        InitializeLevel3();
    }

    /// <summary>
    /// Fuerza la activación visual de todas las partes del robot
    /// Útil para llamar manualmente si las partes no aparecen
    /// </summary>
    public void ForceShowAllParts()
    {
        if (!isLevel3Active) return;

        if (playerController == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                playerController = playerGO.GetComponent<PlayerController>();
            }
        }

        if (playerController == null)
        {
            Debug.LogError("Level3Manager: No se encontró PlayerController para mostrar partes");
            return;
        }

        // Activar todas las partes visualmente
        if (playerController.legsGroup != null)
        {
            playerController.legsGroup.SetActive(true);
        }

        if (playerController.armsGroup != null)
        {
            playerController.armsGroup.SetActive(true);
        }

        if (playerController.torsoGroup != null)
        {
            playerController.torsoGroup.SetActive(true);
        }

        // Asegurar flags
        playerController.hasLegs = true;
        playerController.hasArms = true;
        playerController.hasTorso = true;

        // Actualizar UI
        playerController.UpdateStatusText();
        playerController.UpdateInstructions();

        if (showDebugLogs)
            Debug.Log(">>> Level3Manager: Todas las partes forzadas a mostrarse");
    }

    // Métodos públicos para otras clases que necesiten interactuar con el Level3Manager
    public void OnBossDefeated()
    {
        if (!isLevel3Active) return;

        if (showDebugLogs)
            Debug.Log(">>> Level3Manager: Jefe derrotado - Nivel 3 completado");
        
        // Aquí puedes agregar lógica adicional cuando se derrota al jefe
    }

    public void OnPanelActivated(int panelNumber, int totalPanels)
    {
        if (!isLevel3Active) return;

        if (showDebugLogs)
            Debug.Log($">>> Level3Manager: Panel {panelNumber}/{totalPanels} activado");
        
        // Aquí puedes agregar lógica adicional cuando se activa un panel
    }

    /// <summary>
    /// Alterna el estado de pausa del juego
    /// </summary>
    public void TogglePause()
    {
        if (!isLevel3Active) return;

        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
                Debug.Log("⏸️ Level3Manager: Juego PAUSADO");
            }
            else
            {
                Debug.LogWarning("⚠️ Level3Manager: pauseMenuPanel no está asignado. El juego está pausado pero no hay menú visible.");
            }

            // Mostrar cursor
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1f;
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
            Debug.Log("▶️ Level3Manager: Juego REANUDADO");

            // Ocultar cursor
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    /// <summary>
    /// Reinicia el nivel 3
    /// </summary>
    public void RestartLevel()
    {
        if (!isLevel3Active) return;

        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Carga el menú principal
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}

