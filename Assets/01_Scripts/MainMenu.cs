using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

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

    [Header("Credits Panel")]
    public Button creditsBackButton;
    private Vector3 creditsBackButtonOriginalScale;

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
    private bool isInCredits = false; 

    private void Start()
    {
        if (menuButtons != null && menuButtons.Length > 0)
        {
            originalScales = new Vector3[menuButtons.Length];
            for (int i = 0; i < menuButtons.Length; i++)
            {
                if (menuButtons[i] != null)
                    originalScales[i] = menuButtons[i].transform.localScale;
            }
        }

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

        if (creditsBackButton != null)
        {
            creditsBackButtonOriginalScale = creditsBackButton.transform.localScale;
            creditsBackButton.onClick.RemoveAllListeners();
            creditsBackButton.onClick.AddListener(CloseCredits);
        }

        ShowMainMenu();

        if (menuButtons != null && menuButtons.Length > 0)
            SelectButton(0);
    }

    private void Update()
    {
        if (isInCredits)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (creditsBackButton != null)
                {
                    PlayClickSound();
                    creditsBackButton.onClick.Invoke();
                }
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) ||
                Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            {
                if (creditsBackButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(creditsBackButton.gameObject);
                }
            }

            return;
        }

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

        if (currentSelectedIndex >= 0 && currentSelectedIndex < menuButtons.Length)
        {
            if (menuButtons[currentSelectedIndex] != null)
                StartCoroutine(ScaleButton(menuButtons[currentSelectedIndex].transform, originalScales[currentSelectedIndex]));
        }

        currentSelectedIndex = index;

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
        isInCredits = false;
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    public void OpenCredits()
    {
        PlayClickSound();
        isInCredits = true;

        if (menuButtons != null)
        {
            foreach (Button btn in menuButtons)
            {
                if (btn != null)
                    btn.interactable = false;
            }
        }

        if (mainMenuPanel != null)
        {
            CanvasGroup cg = mainMenuPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = mainMenuPanel.AddComponent<CanvasGroup>();
            cg.alpha = 0;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(true);

        StartCoroutine(SelectCreditsButtonDelayed());
    }

    private IEnumerator SelectCreditsButtonDelayed()
    {
        yield return null;

        if (creditsBackButton != null)
        {
            creditsBackButton.transform.localScale = creditsBackButtonOriginalScale * buttonHoverScale;
            creditsBackButton.interactable = true;

            TextMeshProUGUI buttonText = creditsBackButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = Color.white;
            }

            if (EventSystem.current != null)
            {
                EventSystem.current.sendNavigationEvents = false;
            }

            EventSystem.current.SetSelectedGameObject(null);
            yield return null;
            EventSystem.current.SetSelectedGameObject(creditsBackButton.gameObject);
        }
    }

    public void CloseCredits()
    {
        PlayClickSound();
        isInCredits = false;

        if (EventSystem.current != null)
        {
            EventSystem.current.sendNavigationEvents = true;
        }

        if (menuButtons != null)
        {
            foreach (Button btn in menuButtons)
            {
                if (btn != null)
                    btn.interactable = true;
            }
        }

        if (creditsBackButton != null)
        {
            creditsBackButton.transform.localScale = creditsBackButtonOriginalScale;

            TextMeshProUGUI buttonText = creditsBackButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = Color.white;
            }
        }

        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        isInCredits = false;
        if (mainMenuPanel != null)
        {
            CanvasGroup cg = mainMenuPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            mainMenuPanel.SetActive(true);
        }
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

        yield return new WaitForSeconds(0.3f);

        SceneManager.LoadScene(sceneName);
    }
}