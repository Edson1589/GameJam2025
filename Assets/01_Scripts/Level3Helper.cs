using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Level3Helper - Utilidades estáticas para trabajar con el Nivel 3
/// Usa estos métodos en cualquier script para verificar o trabajar con el Nivel 3
/// sin afectar otros niveles
/// </summary>
public static class Level3Helper
{
    /// <summary>
    /// Verifica si la escena actual es el Nivel 3
    /// </summary>
    public static bool IsCurrentLevel3()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        return IsLevel3Scene(currentScene);
    }

    /// <summary>
    /// Verifica si un nombre de escena corresponde al Nivel 3
    /// </summary>
    public static bool IsLevel3Scene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        
        return sceneName.Contains("Level_03") || 
               sceneName.Contains("Azotea") ||
               sceneName.Contains("Level3");
    }

    /// <summary>
    /// Obtiene el Level3Manager si está activo
    /// </summary>
    public static Level3Manager GetLevel3Manager()
    {
        if (Level3Manager.Instance != null && Level3Manager.Instance.IsActive())
        {
            return Level3Manager.Instance;
        }
        return null;
    }

    /// <summary>
    /// Ejecuta una acción solo si estamos en el Nivel 3
    /// </summary>
    public static void ExecuteIfLevel3(System.Action action)
    {
        if (IsCurrentLevel3() && action != null)
        {
            action.Invoke();
        }
    }

    /// <summary>
    /// Ejecuta una acción solo si estamos en el Nivel 3 y el Level3Manager está activo
    /// </summary>
    public static void ExecuteIfLevel3WithManager(System.Action<Level3Manager> action)
    {
        Level3Manager manager = GetLevel3Manager();
        if (manager != null && action != null)
        {
            action.Invoke(manager);
        }
    }
}

