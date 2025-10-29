using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class PersistentHUD : MonoBehaviour
{
    public static PersistentHUD Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject progressPanel;
    [SerializeField] private GameObject configurationPanel;
    [SerializeField] private GameObject waitingForKeyPanel;

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
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Configuration Panel - Audio")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI musicValueText;
    [SerializeField] private TextMeshProUGUI sfxValueText;

    [Header("Configuration Panel - Key Bindings")]
    [SerializeField] private Button jumpButton;
    [SerializeField] private Button interactButton;
    [SerializeField] private Button dashButton;
    [SerializeField] private Button flashlightButton;
    [SerializeField] private TextMeshProUGUI jumpButtonText;
    [SerializeField] private TextMeshProUGUI interactButtonText;
    [SerializeField] private TextMeshProUGUI dashButtonText;
    [SerializeField] private TextMeshProUGUI flashlightButtonText;

    [Header("Configuration Panel - Other Buttons")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button backFromConfigButton;
    [SerializeField] private TextMeshProUGUI waitingText;

    [Header("Navigation Settings")]
    [SerializeField] private Selectable[] configMenuSelectables;
    [SerializeField] private float buttonHoverScale = 1.1f;
    [SerializeField] private float animationSpeed = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioSource buttonHoverSound;
    [SerializeField] private AudioSource buttonClickSound;

    private bool isPaused = false;
    private bool optionsPanelOpen = false;
    private bool progressPanelOpen = false;
    private bool configurationPanelOpen = false;
    private bool isWaitingForKey = false;
    private string currentActionToRemap = "";
    private int currentSelectedIndex = 0;
    private Vector3[] originalScales;

    void Awake()
    {
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
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (progressPanel != null) progressPanel.SetActive(false);
        if (configurationPanel != null) configurationPanel.SetActive(false);
        if (waitingForKeyPanel != null) waitingForKeyPanel.SetActive(false);

        SetupConfigurationPanel();
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
        RefreshConfigurationValues();
    }

    void Update()
    {
        if (Input.GetKeyDown(optionsKey))
        {
            if (configurationPanelOpen)
            {
                if (isWaitingForKey)
                {
                    CancelRemapping();
                }
                else
                {
                    CloseConfigurationToOptions();
                }
            }
            else if (optionsPanelOpen)
                CloseOptions();
            else
                OpenOptions();
        }

        if (Input.GetKeyDown(progressKey))
        {
            if (progressPanelOpen)
                CloseProgress();
            else
                OpenProgress();
        }

        HandleKeyRemapping();
        HandleConfigNavigation();
    }

    private void SetupConfigurationPanel()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.value = AudioManager.Instance.GetMusicVolume();
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            UpdateMusicText(musicSlider.value);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.value = AudioManager.Instance.GetSFXVolume();
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            UpdateSFXText(sfxSlider.value);
        }

        if (jumpButton != null)
        {
            jumpButton.onClick.RemoveAllListeners();
            jumpButton.onClick.AddListener(() => StartRemapping("Jump"));
        }
        if (interactButton != null)
        {
            interactButton.onClick.RemoveAllListeners();
            interactButton.onClick.AddListener(() => StartRemapping("Interact"));
        }
        if (dashButton != null)
        {
            dashButton.onClick.RemoveAllListeners();
            dashButton.onClick.AddListener(() => StartRemapping("Dash"));
        }
        if (flashlightButton != null)
        {
            flashlightButton.onClick.RemoveAllListeners();
            flashlightButton.onClick.AddListener(() => StartRemapping("Flashlight"));
        }

        if (saveButton != null)
        {
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(SaveSettings);
        }
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(ResetToDefaults);
        }
        if (backFromConfigButton != null)
        {
            backFromConfigButton.onClick.RemoveAllListeners();
            backFromConfigButton.onClick.AddListener(CloseConfigurationToOptions);
        }

        if (configMenuSelectables != null && configMenuSelectables.Length > 0)
        {
            originalScales = new Vector3[configMenuSelectables.Length];
            for (int i = 0; i < configMenuSelectables.Length; i++)
            {
                if (configMenuSelectables[i] != null)
                    originalScales[i] = configMenuSelectables[i].transform.localScale;
            }
        }

        UpdateAllKeyTexts();
    }

    private void RefreshConfigurationValues()
    {
        if (musicSlider != null)
        {
            musicSlider.value = AudioManager.Instance.GetMusicVolume();
            UpdateMusicText(musicSlider.value);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = AudioManager.Instance.GetSFXVolume();
            UpdateSFXText(sfxSlider.value);
        }

        UpdateAllKeyTexts();
    }

    public void OpenOptions()
    {
        if (IsInMainMenu()) return;

        optionsPanelOpen = true;
        if (optionsPanel != null) optionsPanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (resumeButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void CloseOptions()
    {
        optionsPanelOpen = false;
        if (optionsPanel != null) optionsPanel.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OpenConfiguration()
    {
        RefreshConfigurationValues();

        optionsPanelOpen = false;
        if (optionsPanel != null) optionsPanel.SetActive(false);

        configurationPanelOpen = true;
        if (configurationPanel != null) configurationPanel.SetActive(true);

        if (configMenuSelectables != null && configMenuSelectables.Length > 0)
        {
            SelectSelectable(0);
        }
        else if (backFromConfigButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(backFromConfigButton.gameObject);
        }
    }

    public void CloseConfigurationToOptions()
    {
        configurationPanelOpen = false;
        if (configurationPanel != null) configurationPanel.SetActive(false);

        OpenOptions();
    }

    public void CloseConfigurationCompletely()
    {
        configurationPanelOpen = false;
        if (configurationPanel != null) configurationPanel.SetActive(false);

        optionsPanelOpen = false;
        if (optionsPanel != null) optionsPanel.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    private void HandleKeyRemapping()
    {
        if (!isWaitingForKey) return;

        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                if (key >= KeyCode.Mouse0 && key <= KeyCode.Mouse6)
                    continue;

                if (key == KeyCode.Escape || key == KeyCode.Return || key == KeyCode.KeypadEnter)
                {
                    CancelRemapping();
                    return;
                }

                AssignKey(key);
                return;
            }
        }
    }

    private void HandleConfigNavigation()
    {
        if (!configurationPanelOpen || isWaitingForKey) return;
        if (configMenuSelectables == null || configMenuSelectables.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            SelectSelectable((currentSelectedIndex + 1) % configMenuSelectables.Length);
            PlayHoverSound();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            SelectSelectable((currentSelectedIndex - 1 + configMenuSelectables.Length) % configMenuSelectables.Length);
            PlayHoverSound();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            if (configMenuSelectables[currentSelectedIndex] != null)
            {
                Button selectedButton = configMenuSelectables[currentSelectedIndex] as Button;
                if (selectedButton != null)
                {
                    PlayClickSound();
                    selectedButton.onClick.Invoke();
                }
            }
        }
    }

    private void SelectSelectable(int index)
    {
        if (originalScales == null || originalScales.Length == 0) return;

        if (currentSelectedIndex >= 0 && currentSelectedIndex < configMenuSelectables.Length)
        {
            Selectable previousSelectable = configMenuSelectables[currentSelectedIndex];
            if (previousSelectable != null)
            {
                Slider previousSlider = previousSelectable as Slider;
                if (previousSlider != null && previousSlider.targetGraphic != null)
                {
                    previousSlider.targetGraphic.color = Color.white;
                }

                Button previousButton = previousSelectable as Button;
                if (previousButton != null)
                {
                    StartCoroutine(ScaleButton(previousButton.transform, originalScales[currentSelectedIndex]));
                    TextMeshProUGUI previousText = previousButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (previousText != null) previousText.color = Color.white;
                }
            }
        }

        currentSelectedIndex = index;

        Selectable newSelectable = configMenuSelectables[currentSelectedIndex];
        if (newSelectable != null)
        {
            Slider newSlider = newSelectable as Slider;
            if (newSlider != null && newSlider.targetGraphic != null)
            {
                newSlider.targetGraphic.color = Color.white;
            }

            Button newButton = newSelectable as Button;
            if (newButton != null)
            {
                StartCoroutine(ScaleButton(newButton.transform, originalScales[currentSelectedIndex] * buttonHoverScale));

                TextMeshProUGUI newText = newButton.GetComponentInChildren<TextMeshProUGUI>();
                if (newText != null) newText.color = Color.white;
            }

            EventSystem.current.SetSelectedGameObject(newSelectable.gameObject);
        }
    }

    private IEnumerator ScaleButton(Transform button, Vector3 targetScale)
    {
        Vector3 startScale = button.localScale;
        float elapsed = 0f;

        while (elapsed < animationSpeed)
        {
            elapsed += Time.unscaledDeltaTime;
            button.localScale = Vector3.Lerp(startScale, targetScale, elapsed / animationSpeed);
            yield return null;
        }

        button.localScale = targetScale;
    }

    private void PlayHoverSound()
    {
        if (buttonHoverSound != null)
            buttonHoverSound.Play();
    }

    private void PlayClickSound()
    {
        if (buttonClickSound != null)
            buttonClickSound.Play();
    }

    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
        UpdateMusicText(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
        UpdateSFXText(value);
    }

    private void UpdateMusicText(float value)
    {
        if (musicValueText != null)
            musicValueText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    private void UpdateSFXText(float value)
    {
        if (sfxValueText != null)
            sfxValueText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    private void StartRemapping(string actionName)
    {
        EventSystem.current.SetSelectedGameObject(null);

        currentActionToRemap = actionName;
        isWaitingForKey = true;

        if (waitingForKeyPanel != null)
            waitingForKeyPanel.SetActive(true);

        if (waitingText != null)
            waitingText.text = "Press a key to " + GetActionName(actionName) + "\n(ESC to cancel)";
    }

    private void AssignKey(KeyCode newKey)
    {
        string conflictingAction = null;

        foreach (var binding in InputManager.Instance.keyBindings)
        {
            if (binding.currentKey == newKey && binding.actionName != currentActionToRemap)
            {
                conflictingAction = binding.actionName;
                break;
            }
        }

        if (conflictingAction != null)
        {
            if (waitingText != null)
            {
                StartCoroutine(ShowErrorMessage("Key already used by " + GetActionName(conflictingAction) + "!\nPress another key."));
            }
            return;
        }

        InputManager.Instance.SetKeyForAction(currentActionToRemap, newKey);
        UpdateAllKeyTexts();
        CancelRemapping();
    }

    private IEnumerator ShowErrorMessage(string message)
    {
        if (waitingText != null)
        {
            waitingText.text = message;
            waitingText.color = Color.red;
            yield return new WaitForSecondsRealtime(1.5f);
            waitingText.color = Color.white;
            waitingText.text = "Press a key to " + GetActionName(currentActionToRemap) + "\n(ESC to cancel)";
        }
    }

    private void CancelRemapping()
    {
        isWaitingForKey = false;
        currentActionToRemap = "";

        if (waitingForKeyPanel != null)
            waitingForKeyPanel.SetActive(false);
    }

    private void UpdateAllKeyTexts()
    {
        if (jumpButtonText != null)
            jumpButtonText.text = GetKeyDisplayName(InputManager.Instance.GetKeyForAction("Jump"));
        if (interactButtonText != null)
            interactButtonText.text = GetKeyDisplayName(InputManager.Instance.GetKeyForAction("Interact"));
        if (dashButtonText != null)
            dashButtonText.text = GetKeyDisplayName(InputManager.Instance.GetKeyForAction("Dash"));
        if (flashlightButtonText != null)
            flashlightButtonText.text = GetKeyDisplayName(InputManager.Instance.GetKeyForAction("Flashlight"));
    }

    private string GetKeyDisplayName(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.LeftShift: return "Left Shift";
            case KeyCode.RightShift: return "Right Shift";
            case KeyCode.LeftControl: return "Left Control";
            case KeyCode.RightControl: return "Right Control";
            case KeyCode.Space: return "Space";
            default: return key.ToString().ToUpper();
        }
    }

    private string GetActionName(string actionName)
    {
        switch (actionName)
        {
            case "Jump": return "Jump";
            case "Interact": return "Interact";
            case "Dash": return "Dash";
            case "Flashlight": return "Flashlight";
            default: return actionName;
        }
    }

    private void SaveSettings()
    {
        AudioManager.Instance.SaveAudioSettings();
        InputManager.Instance.SaveKeyBindings();
        Debug.Log("Settings saved");
    }

    private void ResetToDefaults()
    {
        if (musicSlider != null)
        {
            musicSlider.value = 0.75f;
            AudioManager.Instance.SetMusicVolume(0.75f);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = 0.75f;
            AudioManager.Instance.SetSFXVolume(0.75f);
        }

        InputManager.Instance.ResetToDefaults();
        UpdateAllKeyTexts();

        Debug.Log("Settings reset to defaults");
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
            }
            else if (currentScene.Contains("Level_02") || currentScene.Contains("Tapes"))
            {
                GameManager.Instance.hasArms = false;
            }
            else if (currentScene.Contains("Level_03") || currentScene.Contains("Azotea"))
            {
                GameManager.Instance.hasTorso = false;
            }

            GameManager.Instance.ResetMemoriesForLevel(currentScene);
        }

        SceneManager.LoadScene(currentScene);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene("MainMenu");
    }

    public void OpenProgress()
    {
        if (IsInMainMenu()) return;

        progressPanelOpen = true;
        if (progressPanel != null) progressPanel.SetActive(true);

        UpdateProgressInfo();
    }

    public void CloseProgress()
    {
        progressPanelOpen = false;
        if (progressPanel != null) progressPanel.SetActive(false);
    }

    private void UpdateProgressInfo()
    {
        if (currentLevelText != null)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            currentLevelText.text = $"CURRENT LEVEL: {GetLevelDisplayName(sceneName)}";
        }

        if (partsStatusText != null && GameManager.Instance != null)
        {
            string parts = "PARTS OF R.U.B.O:\n\n";

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
        configurationPanelOpen = false;
        if (configurationPanel != null) configurationPanel.SetActive(false);
    }

    private void UpdateVisibility()
    {
        bool isMainMenu = IsInMainMenu();
        bool isVideo = IsInVideos();

        if (hideInMainMenu && isMainMenu || hideInVideo && isVideo)
        {
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