using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string level01SceneName = "Level_01_Ensamblee";
    [SerializeField] private string testSceneName = "Level_Test";
    [SerializeField] private string videoSceneName = "VideoScene";

    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Buttons")]
    public Button playButton;
    public Button playTestButton;
    public Button optionsButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Button Selection")]
    public Button[] menuButtons;
    private int currentSelectedIndex = 0;

    [Header("Animations")]
    public float buttonHoverScale = 1.1f;
    public float animationSpeed = 0.2f;

    [Header("Audio")]
    public AudioSource buttonHoverSound;
    public AudioSource buttonClickSound;

    private Vector3[] originalScales;
    private bool canNavigate = true;

    private void Start()
    {
        // Guardar escalas originales de los botones
        if (menuButtons != null && menuButtons.Length > 0)
        {
            originalScales = new Vector3[menuButtons.Length];
            for (int i = 0; i < menuButtons.Length; i++)
            {
                if (menuButtons[i] != null)
                    originalScales[i] = menuButtons[i].transform.localScale;
            }
        }

        // Configurar listeners de botones
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);
        if (playTestButton != null)
            playTestButton.onClick.AddListener(PlayTestLevel);
        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenOptions);
        if (creditsButton != null)
            creditsButton.onClick.AddListener(OpenCredits);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // Mostrar solo el menú principal al inicio
        ShowMainMenu();

        // Seleccionar primer botón si usas navegación por teclado
        if (menuButtons != null && menuButtons.Length > 0)
            SelectButton(0);
    }

    private void Update()
    {
        // Navegación con teclado
        if (!canNavigate || menuButtons == null || menuButtons.Length == 0)
            return;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            SelectButton((currentSelectedIndex + 1) % menuButtons.Length);
            PlayHoverSound();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            SelectButton((currentSelectedIndex - 1 + menuButtons.Length) % menuButtons.Length);
            PlayHoverSound();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (menuButtons[currentSelectedIndex] != null)
            {
                PlayClickSound();
                menuButtons[currentSelectedIndex].onClick.Invoke();
            }
        }
    }

    private void SelectButton(int index)
    {
        if (originalScales == null || originalScales.Length == 0) return;

        // Resetear escala del botón anterior
        if (currentSelectedIndex >= 0 && currentSelectedIndex < menuButtons.Length)
        {
            if (menuButtons[currentSelectedIndex] != null)
                StartCoroutine(ScaleButton(menuButtons[currentSelectedIndex].transform, originalScales[currentSelectedIndex]));
        }

        currentSelectedIndex = index;

        // Escalar nuevo botón
        if (menuButtons[currentSelectedIndex] != null)
        {
            StartCoroutine(ScaleButton(menuButtons[currentSelectedIndex].transform,
                originalScales[currentSelectedIndex] * buttonHoverScale));
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

    public void PlayGame()
    {
        Debug.Log("Reproduciendo video inicial...");
        PlayClickSound();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetProgress();
        }

        StartCoroutine(LoadSceneWithDelay(videoSceneName));
    }

    public void PlayTestLevel()
    {
        Debug.Log("Cargando nivel de prueba - Reseteando progreso...");
        PlayClickSound();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetProgress();
        }

        StartCoroutine(LoadSceneWithDelay(testSceneName));
    }

    public void OpenOptions()
    {
        PlayClickSound();
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    public void OpenCredits()
    {
        PlayClickSound();
        Debug.Log("Créditos - Desarrollado para GameJam 2025");

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        PlayClickSound();
        Debug.Log("Saliendo del juego...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private IEnumerator LoadSceneWithDelay(string sceneName)
    {
        canNavigate = false;

        // Pequeño delay para que se escuche el sonido del click
        yield return new WaitForSeconds(0.3f);

        SceneManager.LoadScene(sceneName);
    }
}