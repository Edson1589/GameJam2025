using UnityEngine;

public class LevelExit : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatSpeed = 1.5f;

    private Vector3 startPosition;
    private bool levelComplete = false;

    void Start()
    {
        startPosition = transform.position;
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
            // Verificar que tenga las piernas (opcional)
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
        Debug.Log("=== ¡NIVEL 1 COMPLETADO! ===");
        Debug.Log("¡Has recuperado tus PIERNAS y escapado de la Zona de Ensamble!");

        //Reiniciar después de 3 segundos
        Invoke("RestartLevel", 3f);
    }

    private void RestartLevel()
    {
        // Recargar la escena actual
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}