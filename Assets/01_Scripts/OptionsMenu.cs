using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

public class OptionsMenu : MonoBehaviour
{
    [Header("Audio Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public TextMeshProUGUI musicValueText;
    public TextMeshProUGUI sfxValueText;

    [Header("Key Binding Buttons")]
    public Button jumpButton;
    public Button interactButton;
    public Button dashButton;
    public Button flashlightButton;

    [Header("Button Texts")]
    public TextMeshProUGUI jumpButtonText;
    public TextMeshProUGUI interactButtonText;
    public TextMeshProUGUI dashButtonText;
    public TextMeshProUGUI flashlightButtonText;

    [Header("Colors")]
    public Color normalTextColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    public Color highlightedTextColor = Color.white;

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;
    public GameObject waitingForKeyPanel;
    public TextMeshProUGUI waitingText;

    [Header("Localized Strings - Key Remapping")]
    [SerializeField] private LocalizedString localizedPressKeyFor;
    [SerializeField] private LocalizedString localizedKeyConflict;
    [SerializeField] private LocalizedString localizedActionJump;
    [SerializeField] private LocalizedString localizedActionInteract;
    [SerializeField] private LocalizedString localizedActionDash;
    [SerializeField] private LocalizedString localizedActionFlashlight;

    [Header("Other Buttons")]
    public Button saveButton;
    public Button resetButton;
    public Button backButton;

    [Header("Navigation Settings")]
    public Selectable[] optionsMenuSelectables;
    public float buttonHoverScale = 1.1f;
    public float animationSpeed = 0.2f;

    [Header("Audio")]
    public AudioSource buttonHoverSound;
    public AudioSource buttonClickSound;

    private string currentActionToRemap = "";
    private bool isWaitingForKey = false;

    private int currentSelectedIndex = 0;
    private Vector3[] originalScales;

    private void Start()
    {
        if (musicSlider != null)
        {
            musicSlider.value = AudioManager.Instance.GetMusicVolume();
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            UpdateMusicText(musicSlider.value);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = AudioManager.Instance.GetSFXVolume();
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            UpdateSFXText(sfxSlider.value);
        }

        InitializeButtonColors();

        if (jumpButton != null)
            jumpButton.onClick.AddListener(() => StartRemapping("Jump"));
        if (interactButton != null)
            interactButton.onClick.AddListener(() => StartRemapping("Interact"));
        if (dashButton != null)
            dashButton.onClick.AddListener(() => StartRemapping("Dash"));
        if (flashlightButton != null)
            flashlightButton.onClick.AddListener(() => StartRemapping("Flashlight"));

        if (saveButton != null)
            saveButton.onClick.AddListener(SaveSettings);
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetToDefaults);
        if (backButton != null)
            backButton.onClick.AddListener(CloseOptions);

        if (waitingForKeyPanel != null)
            waitingForKeyPanel.SetActive(false);

        if (optionsMenuSelectables != null && optionsMenuSelectables.Length > 0)
        {
            originalScales = new Vector3[optionsMenuSelectables.Length];
            for (int i = 0; i < optionsMenuSelectables.Length; i++)
            {
                if (optionsMenuSelectables[i] != null)
                    originalScales[i] = optionsMenuSelectables[i].transform.localScale;
            }
            SelectSelectable(0);
        }

        UpdateAllKeyTexts();
    }

    private void Update()
    {
        if (optionsPanel != null && !optionsPanel.activeSelf)
            return;

        if (isWaitingForKey)
        {
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
            return;
        }

        if (optionsMenuSelectables == null || optionsMenuSelectables.Length == 0)
            return;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            SelectSelectable((currentSelectedIndex + 1) % optionsMenuSelectables.Length);
            PlayHoverSound();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            SelectSelectable((currentSelectedIndex - 1 + optionsMenuSelectables.Length) % optionsMenuSelectables.Length);
            PlayHoverSound();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            Selectable currentSelectable = optionsMenuSelectables[currentSelectedIndex];
            if (currentSelectable != null)
            {
                PlayClickSound();
                UnityEngine.EventSystems.ExecuteEvents.Execute(currentSelectable.gameObject, new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current), UnityEngine.EventSystems.ExecuteEvents.submitHandler);
            }
        }

        bool isHorizontalNavigation = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D);

        if (isHorizontalNavigation)
        {
            int saveIndex = 6;
            int resetIndex = 7;
            int backIndex = 8;

            bool canNavigateHorizontally = (currentSelectedIndex >= saveIndex && currentSelectedIndex <= backIndex);

            if (canNavigateHorizontally)
            {
                int newIndex = currentSelectedIndex;
                bool shouldMove = false;

                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    if (currentSelectedIndex < backIndex)
                    {
                        newIndex = currentSelectedIndex + 1;
                        shouldMove = true;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    if (currentSelectedIndex > saveIndex)
                    {
                        newIndex = currentSelectedIndex - 1;
                        shouldMove = true;
                    }
                }

                if (shouldMove)
                {
                    SelectSelectable(newIndex);
                    PlayHoverSound();
                }
            }
        }
    }

    private void InitializeButtonColors()
    {
        if (jumpButtonText != null) jumpButtonText.color = normalTextColor;
        if (interactButtonText != null) interactButtonText.color = normalTextColor;
        if (dashButtonText != null) dashButtonText.color = normalTextColor;
        if (flashlightButtonText != null) flashlightButtonText.color = normalTextColor;

        if (saveButton != null)
        {
            TextMeshProUGUI text = saveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.color = normalTextColor;
        }
        if (resetButton != null)
        {
            TextMeshProUGUI text = resetButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.color = normalTextColor;
        }
        if (backButton != null)
        {
            TextMeshProUGUI text = backButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.color = normalTextColor;
        }
    }

    private void SelectSelectable(int index)
    {
        if (originalScales == null || originalScales.Length == 0) return;

        if (currentSelectedIndex >= 0 && currentSelectedIndex < optionsMenuSelectables.Length)
        {
            Selectable previousSelectable = optionsMenuSelectables[currentSelectedIndex];
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
                    if (previousText != null)
                        previousText.color = normalTextColor;
                }
            }
        }

        currentSelectedIndex = index;
        Selectable newSelectable = optionsMenuSelectables[currentSelectedIndex];
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
                StartCoroutine(ScaleButton(newButton.transform,
                    originalScales[currentSelectedIndex] * buttonHoverScale));

                TextMeshProUGUI newText = newButton.GetComponentInChildren<TextMeshProUGUI>();
                if (newText != null)
                    newText.color = highlightedTextColor;
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
            elapsed += Time.deltaTime;
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

        // Usar localización si está disponible
        if (waitingText != null && localizedPressKeyFor != null)
        {
            localizedPressKeyFor.Arguments = new object[] { GetLocalizedActionName(actionName) };
            waitingText.text = localizedPressKeyFor.GetLocalizedString();
        }
        else if (waitingText != null)
        {
            // Fallback sin localización
            waitingText.text = "Press a key to " + GetActionName(actionName) + "\n(ESC to cancel)";
        }
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
            Debug.LogWarning("Key " + newKey.ToString() + " already assigned to: " + conflictingAction);

            if (waitingText != null && localizedKeyConflict != null)
            {
                StartCoroutine(ShowErrorMessage(conflictingAction));
            }
            else if (waitingText != null)
            {
                // Fallback sin localización
                waitingText.text = "Error! Key already used by " + GetActionName(conflictingAction) + ".\nPress another key.";
            }

            return;
        }

        InputManager.Instance.SetKeyForAction(currentActionToRemap, newKey);
        UpdateAllKeyTexts();
        CancelRemapping();
    }

    private IEnumerator ShowErrorMessage(string conflictingAction)
    {
        if (waitingText != null && localizedKeyConflict != null && localizedPressKeyFor != null)
        {
            // Mostrar error en rojo
            localizedKeyConflict.Arguments = new object[] { GetLocalizedActionName(conflictingAction) };
            waitingText.text = localizedKeyConflict.GetLocalizedString();
            waitingText.color = Color.red;

            yield return new WaitForSeconds(1.5f);

            // Volver al mensaje normal
            waitingText.color = Color.white;
            localizedPressKeyFor.Arguments = new object[] { GetLocalizedActionName(currentActionToRemap) };
            waitingText.text = localizedPressKeyFor.GetLocalizedString();
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

    private string GetLocalizedActionName(string actionName)
    {
        switch (actionName)
        {
            case "Jump":
                return localizedActionJump != null ? localizedActionJump.GetLocalizedString() : "Jump";
            case "Interact":
                return localizedActionInteract != null ? localizedActionInteract.GetLocalizedString() : "Interact";
            case "Dash":
                return localizedActionDash != null ? localizedActionDash.GetLocalizedString() : "Dash";
            case "Flashlight":
                return localizedActionFlashlight != null ? localizedActionFlashlight.GetLocalizedString() : "Flashlight";
            default:
                return actionName;
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
        Debug.Log("Configuración guardada");
    }

    private void ResetToDefaults()
    {
        musicSlider.value = 0.75f;
        sfxSlider.value = 0.75f;
        AudioManager.Instance.SetMusicVolume(0.75f);
        AudioManager.Instance.SetSFXVolume(0.75f);

        InputManager.Instance.ResetToDefaults();
        UpdateAllKeyTexts();

        Debug.Log("Configuración reseteada a valores predeterminados");
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        MainMenu mainMenu = FindObjectOfType<MainMenu>();
        if (mainMenu != null)
        {
            mainMenu.ShowMainMenu();
        }
    }

    public void OpenOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        if (optionsMenuSelectables != null && optionsMenuSelectables.Length > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);

            int BackButtonIndex = -1;

            if (backButton != null)
            {
                BackButtonIndex = System.Array.IndexOf(optionsMenuSelectables, backButton);
            }

            if (BackButtonIndex >= 0)
            {
                SelectSelectable(BackButtonIndex);
            }
            else
            {
                SelectSelectable(0);
            }
        }
    }
}