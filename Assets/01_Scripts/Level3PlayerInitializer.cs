using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Level3PlayerInitializer - Inicializa el jugador específicamente para el Nivel 3
/// Ahora usa Level3Manager para centralizar la lógica
/// </summary>
public class Level3PlayerInitializer : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private bool useLevel3Manager = true;

    void Start()
    {
        // Verificar que estamos en el nivel 3
        string currentScene = SceneManager.GetActiveScene().name;
        if (!Level3Manager.IsLevel3Scene(currentScene))
        {
            Debug.LogWarning($"Level3PlayerInitializer detectado fuera del Nivel 3: {currentScene}. Desactivando...");
            enabled = false;
            return;
        }

        // Usar Level3Manager si está disponible
        if (useLevel3Manager && Level3Manager.Instance != null)
        {
            // El Level3Manager se encargará de la inicialización
            return;
        }

        // Fallback: inicialización manual si Level3Manager no está disponible
        if (playerHealth != null)
        {
            // true = tiene Legs, Arms, Torso
            playerHealth.InitializeFromParts(true, true, true);
        }
        else
        {
            Debug.LogError("PlayerHealth no asignado en Level3PlayerInitializer");
        }
    }
}