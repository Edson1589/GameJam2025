using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string level01SceneName = "Level_01_Ensamble";
    [SerializeField] private string testSceneName = "Level_Test";

    public void PlayGame()
    {
        Debug.Log("Iniciando juego...");
        SceneManager.LoadScene(level01SceneName);
    }

    public void PlayTestLevel()
    {
        Debug.Log("Cargando nivel de prueba...");
        SceneManager.LoadScene(testSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();

        // Para testing en editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OpenCredits()
    {
        Debug.Log("Créditos - Grupo Pixels - Desarrollado para GameJam 2025");
    }
}