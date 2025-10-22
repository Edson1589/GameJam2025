using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Level Progression")]
    [SerializeField] private string nextLevelName = "";
    [SerializeField] private string mainMenuName = "MainMenu";

    [Header("UI (Optional)")]
    [SerializeField] private GameObject pauseMenuPanel;

    private bool isPaused = false;

    void Update()
    {
        // Pausa con ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            Debug.Log($"Cargando siguiente nivel: {nextLevelName}");
            SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            Debug.LogWarning("No hay siguiente nivel configurado");
        }
    }

    public void RestartLevel()
    {
        Debug.Log("Reiniciando nivel...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Debug.Log("Volviendo al menú principal...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuName);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            Debug.Log("Juego PAUSADO");
        }
        else
        {
            Time.timeScale = 1f;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            Debug.Log("Juego REANUDADO");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}