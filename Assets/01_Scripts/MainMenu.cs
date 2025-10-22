using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string level01SceneName = "Level_01_Ensamble";
    [SerializeField] private string testSceneName = "Level_Test";

    public void PlayGame()
    {
        Debug.Log("Iniciando juego NUEVO - Reseteando progreso...");

        // ← NUEVO: Resetear progreso al iniciar nuevo juego
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetProgress();
        }

        SceneManager.LoadScene(level01SceneName);
    }

    public void PlayTestLevel()
    {
        Debug.Log("Cargando nivel de prueba - Reseteando progreso...");

        // ← NUEVO: Resetear para test
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetProgress();
        }

        SceneManager.LoadScene(testSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OpenCredits()
    {
        Debug.Log("Créditos - Desarrollado para GameJam 2025");
    }
}