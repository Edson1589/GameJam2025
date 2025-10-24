using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PersistentHUD : MonoBehaviour
{
    public static PersistentHUD Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject progressPanel;

    [Header("Progress Panel Elements")]
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI partsStatusText;
    [SerializeField] private TextMeshProUGUI ecoMemoriesText;
    [SerializeField] private TextMeshProUGUI instructionsText;

    [Header("Settings")]
    [SerializeField] private bool hideInMainMenu = true;
    [SerializeField] private bool hideInVideo = true;
    [SerializeField] private KeyCode optionsKey = KeyCode.Escape;
    [SerializeField] private KeyCode progressKey = KeyCode.Tab;

    [Header("Options Panel Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    private bool isPaused = false;
    private bool optionsPanelOpen = false;
    private bool progressPanelOpen = false;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Cerrar paneles al inicio
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (progressPanel != null) progressPanel.SetActive(false);

        UpdateVisibility();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateVisibility();
        CloseAllPanels();
    }

    void Update()
    {
        // Toggle Options con ESC
        if (Input.GetKeyDown(optionsKey))
        {
            if (optionsPanelOpen)
                CloseOptions();
            else
                OpenOptions();
        }

        // Toggle Progress con TAB
        if (Input.GetKeyDown(progressKey))
        {
            if (progressPanelOpen)
                CloseProgress();
            else
                OpenProgress();
        }
    }


    public void OpenOptions()
    {
        if (IsInMainMenu()) return;

        optionsPanelOpen = true;
        if (optionsPanel != null) optionsPanel.SetActive(true);

        // Activar cursor para usar mouse
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Seleccionar primer botón automáticamente para navegación con teclado
        if (resumeButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }

        // Pausar juego
        Time.timeScale = 0f;
        isPaused = true;

        Debug.Log("🔴 Juego PAUSADO - Cursor activo");
    }

    public void CloseOptions()
    {
        optionsPanelOpen = false;
        if (optionsPanel != null) optionsPanel.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Limpiar selección de UI
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // Reanudar juego
        Time.timeScale = 1f;
        isPaused = false;

        Debug.Log("🟢 Juego REANUDADO");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        isPaused = false;

        string currentScene = SceneManager.GetActiveScene().name;

        if (GameManager.Instance != null)
        {
            if (currentScene.Contains("Level_01") || currentScene.Contains("Ensamble"))
            {
                GameManager.Instance.hasLegs = false;
                Debug.Log($"Resetting only LEGS for Level 1.");
            }
            else if (currentScene.Contains("Level_02") || currentScene.Contains("Tapes"))
            {
                GameManager.Instance.hasArms = false;
                Debug.Log($"Resetting only ARMS for Level 2.");
            }
            else if (currentScene.Contains("Level_03") || currentScene.Contains("Azotea"))
            {
                GameManager.Instance.hasTorso = false;
                Debug.Log($"Resetting only TORSO for Level 3.");
            }

            GameManager.Instance.ResetMemoriesForLevel(currentScene);

            Debug.Log($"Progreso del nivel {currentScene} reseteado (parte + memorias del nivel)");
        }

        Debug.Log($"Reiniciando nivel: {currentScene}");
        SceneManager.LoadScene(currentScene);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        Debug.Log("Volviendo al menú principal...");
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenProgress()
    {
        if (IsInMainMenu()) return;

        progressPanelOpen = true;
        if (progressPanel != null) progressPanel.SetActive(true);

        UpdateProgressInfo();

        // NO pausar el juego, solo mostrar info
        Debug.Log("Panel de progreso ABIERTO");
    }

    public void CloseProgress()
    {
        progressPanelOpen = false;
        if (progressPanel != null) progressPanel.SetActive(false);

        Debug.Log("Panel de progreso CERRADO");
    }

    private void UpdateProgressInfo()
    {
        // Nivel actual
        if (currentLevelText != null)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            currentLevelText.text = $"CURRENT LEVEL: {GetLevelDisplayName(sceneName)}";
        }

        // Estado de partes
        if (partsStatusText != null && GameManager.Instance != null)
        {
            string parts = "PARTS OF R.U.B.O:\n\n";

            // Con colores TextMeshPro
            if (GameManager.Instance.hasLegs)
                parts += "<color=#00FF00>LEGS: Connected</color>\n";
            else
                parts += "<color=#888888>LEGS: Disconnected</color>\n";

            if (GameManager.Instance.hasArms)
                parts += "<color=#00FF00>ARMS: Connected</color>\n";
            else
                parts += "<color=#888888>ARMS: Disconnected</color>\n";

            if (GameManager.Instance.hasTorso)
                parts += "<color=#00FF00>TORSO: Connected</color>";
            else
                parts += "<color=#888888>TORSO: Disconnected</color>";

            partsStatusText.text = parts;
        }

        // Eco-Memorias
        if (ecoMemoriesText != null)
        {
            MemoryManager memoryManager = FindObjectOfType<MemoryManager>();
            if (memoryManager != null)
            {
                int collected = memoryManager.GetCollectedCount();
                int total = memoryManager.GetTotalCount();
                ecoMemoriesText.text = $"ECO-MEMORIES: {collected}/{total}";
            }
            else
            {
                ecoMemoriesText.text = "ECO-MEMORIES: 0/9";
            }
        }

        // Instrucciones del nivel
        if (instructionsText != null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                PlayerController player = playerGO.GetComponent<PlayerController>();
                if (player != null)
                {
                    if (!player.hasLegs)
                        instructionsText.text = "GOAL:\nFind your LEGS in the Assembly Zone";
                    else if (!player.hasArms)
                        instructionsText.text = "GOAL:\nFind your ARMS in Tapes and Packaging";
                    else if (!player.hasTorso)
                        instructionsText.text = "GOAL:\nFind your TORSO on the Rooftop";
                    else
                        instructionsText.text = "GOAL:\nDEFEAT A.N.C.L.A.!";
                }
            }
        }
    }

    private void CloseAllPanels()
    {
        CloseOptions();
        CloseProgress();
    }

    private void UpdateVisibility()
    {
        bool isMainMenu = IsInMainMenu();
        bool isVideo = IsInVideos();

        if (hideInMainMenu && isMainMenu || hideInVideo && isVideo)
        {
            // Ocultar HUD en el menú principal
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private bool IsInVideos()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName.Contains("Video") || sceneName.Contains("VideoScene");
    }

    private bool IsInMainMenu()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName.Contains("Menu") || sceneName.Contains("MainMenu");
    }

    private string GetLevelDisplayName(string sceneName)
    {
        if (sceneName.Contains("Level_01") || sceneName.Contains("Ensamble"))
            return "1 - Assembly Zone";
        else if (sceneName.Contains("Level_02") || sceneName.Contains("Tapes"))
            return "2 - Tapes and Packaging";
        else if (sceneName.Contains("Level_03") || sceneName.Contains("Azotea"))
            return "3 - Rooftop-Heliport";
        else if (sceneName.Contains("Test"))
            return "TEST LEVEL";
        else
            return sceneName;
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}