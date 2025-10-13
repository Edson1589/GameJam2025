using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelExit : MonoBehaviour
{
    [Header("Next Level")]
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField] private bool hasNextLevel = false;
    [Header("Visual Feedback")]
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatSpeed = 1.5f;
    private Vector3 startPosition;
    private bool levelComplete = false;
    void Start()
    {
        startPosition = transform.position;
        // Debug para verificar configuración
        Debug.Log($"Exit_Portal configurado - Siguiente nivel: {nextSceneName} | Tiene siguiente nivel: {hasNextLevel}");
    }
    void Update()
    {
        if (levelComplete) return;
        // Rotar
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        // Flotar
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (levelComplete) return;
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log("¡Jugador entró en Exit_Portal!");
            // Verificar que tenga las piernas (opcional - comentar si quieres poder salir sin ellas)
            if (player.hasLegs)
            {
                CompleteLevel();
            }
            else
            {
                Debug.Log("¡Necesitas tus PIERNAS para salir!");
            }
        }
    }
    private void CompleteLevel()
    {
        levelComplete = true;
        Debug.Log("=== ¡NIVEL COMPLETADO! ===");
        if (hasNextLevel && !string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"Cargando siguiente nivel: {nextSceneName}");
            Invoke("LoadNextLevel", 2f);
        }
        else
        {
            Debug.Log("Volviendo al menú principal en 3 segundos...");
            Invoke("ReturnToMenu", 3f);
        }
    }
    private void LoadNextLevel()
    {
        Debug.Log($">>> Intentando cargar: {nextSceneName}");
        // Verificar si la escena existe en Build Settings
        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError($"ERROR: La escena '{nextSceneName}' no existe o no está en Build Settings!");
            ReturnToMenu();
        }
    }
    private void ReturnToMenu()
    {
        Debug.Log(">>> Cargando MainMenu");
        SceneManager.LoadScene("MainMenu");
    }
}