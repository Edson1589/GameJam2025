using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems; 

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

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;
    public GameObject waitingForKeyPanel;
    public TextMeshProUGUI waitingText;

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
        // Configurar sliders de audio
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

        // Configurar botones de teclas
        if (jumpButton != null)
            jumpButton.onClick.AddListener(() => StartRemapping("Jump"));
        if (interactButton != null)
            interactButton.onClick.AddListener(() => StartRemapping("Interact"));
        if (dashButton != null)
            dashButton.onClick.AddListener(() => StartRemapping("Dash"));
        if (flashlightButton != null)
            flashlightButton.onClick.AddListener(() => StartRemapping("Flashlight"));

        // Otros botones
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveSettings);
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetToDefaults);
        if (backButton != null)
            backButton.onClick.AddListener(CloseOptions);

        // Ocultar panel de espera
        if (waitingForKeyPanel != null)
            waitingForKeyPanel.SetActive(false);

        // Inicialización de la navegación
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
        if (isWaitingForKey)
        {
            // Detectar cualquier tecla presionada
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    // Ignorar botones del mouse
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
            if (optionsMenuSelectables[currentSelectedIndex] != null)
            {
                // Solo invocamos la acción si es un botón
                Button selectedButton = optionsMenuSelectables[currentSelectedIndex] as Button;
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

        // 1. Resetear escala y color del anterior
        if (currentSelectedIndex >= 0 && currentSelectedIndex < optionsMenuSelectables.Length)
        {
            Selectable previousSelectable = optionsMenuSelectables[currentSelectedIndex];
            if (previousSelectable != null)
            {
                // Resetear el color del Slider
                Slider previousSlider = previousSelectable as Slider;
                if (previousSlider != null && previousSlider.targetGraphic != null)
                {
                    // Restauramos el color original
                    previousSlider.targetGraphic.color = Color.white;
                }

                // Resetear escala y color del Botón
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

        Selectable newSelectable = optionsMenuSelectables[currentSelectedIndex];
        if (newSelectable != null)
        {
            // Aplicar Highlight al Slider
            Slider newSlider = newSelectable as Slider;
            if (newSlider != null && newSlider.targetGraphic != null)
            {
                newSlider.targetGraphic.color = Color.white; 
            }

            // Aplicar animación al Botón
            Button newButton = newSelectable as Button;
            if (newButton != null)
            {
                StartCoroutine(ScaleButton(newButton.transform,
                    originalScales[currentSelectedIndex] * buttonHoverScale));

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

        if (waitingText != null)
            waitingText.text = "Press a key to " + GetActionName(actionName) + "\n(ESC to cancel)";
    }

    private void AssignKey(KeyCode newKey)
    {
        // 1. Verificar si la tecla ya está asignada a otra acción
        foreach (var binding in InputManager.Instance.keyBindings)
        {
            // Si la tecla ya está asignada a OTRA acción
            if (binding.currentKey == newKey && binding.actionName != currentActionToRemap)
            {
                Debug.LogWarning("Key " + newKey.ToString() + " already assigned to: " + binding.actionName);

                // Mostrar una advertencia al usuario y NO asignar la tecla
                if (waitingText != null)
                {
                    waitingText.text = "Error! Key already used by " + GetActionName(binding.actionName) + ".\nPress another key.";
                }

                return;
            }
        }
        InputManager.Instance.SetKeyForAction(currentActionToRemap, newKey);
        UpdateAllKeyTexts();
        CancelRemapping();
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
        Debug.Log("Configuración guardada");
        // Podrías mostrar un mensaje de confirmación aquí
    }

    private void ResetToDefaults()
    {
        // Resetear audio
        musicSlider.value = 0.75f;
        sfxSlider.value = 0.75f;
        AudioManager.Instance.SetMusicVolume(0.75f);
        AudioManager.Instance.SetSFXVolume(0.75f);

        // Resetear teclas
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