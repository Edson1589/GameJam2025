using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton que persiste entre escenas y guarda el progreso del jugador
/// </summary>
public class GameManager : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════
    // SINGLETON - Solo puede existir uno en todo el juego
    // ═══════════════════════════════════════════════════════
    public static GameManager Instance { get; private set; }

    [Header("Player Progress - R.U.B.O. Parts")]
    public bool hasTorso = false;
    public bool hasLegs = false;
    public bool hasArms = false;
    public int coinsCollected = 0;
    public int currentLevel = 1;
    public float health = 100f;
    public float maxHealth = 100f;

    [Header("Custom Variables")]
    [Tooltip("Agrega aquí variables personalizadas que necesites guardar")]
    public int customNumber = 0;
    public string customText = "";
    public bool customFlag = false;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        // Implementar Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // No destruir al cambiar de escena

        if (showDebugLogs)
            Debug.Log("GameManager inicializado - Este objeto persistirá entre escenas");

        // Cargar datos guardados al iniciar
        LoadGame();
    }

    // ═══════════════════════════════════════════════════════
    // MÉTODOS PÚBLICOS PARA MODIFICAR EL PROGRESO
    // ═══════════════════════════════════════════════════════

    public void UnlockTorso()
    {
        hasTorso = true;
        SaveGame();
        if (showDebugLogs) Debug.Log("✓ Torso desbloqueado!");
    }

    public void UnlockLegs()
    {
        hasLegs = true;
        SaveGame();
        if (showDebugLogs) Debug.Log("✓ Piernas desbloqueadas!");
    }

    public void UnlockArms()
    {
        hasArms = true;
        SaveGame();
        if (showDebugLogs) Debug.Log("✓ Brazos desbloqueados!");
    }

    public void AddCoins(int amount)
    {
        coinsCollected += amount;
        SaveGame();
        if (showDebugLogs) Debug.Log($"Monedas: {coinsCollected} (+{amount})");
    }

    public void SetHealth(float newHealth)
    {
        health = Mathf.Clamp(newHealth, 0f, maxHealth);
        SaveGame();
        if (showDebugLogs) Debug.Log($"Salud: {health}/{maxHealth}");
    }

    public void TakeDamage(float damage)
    {
        SetHealth(health - damage);
    }

    public void Heal(float amount)
    {
        SetHealth(health + amount);
    }

    public void SetCustomNumber(int value)
    {
        customNumber = value;
        SaveGame();
        if (showDebugLogs) Debug.Log($"Custom Number: {customNumber}");
    }

    public void SetCustomText(string text)
    {
        customText = text;
        SaveGame();
        if (showDebugLogs) Debug.Log($"Custom Text: {customText}");
    }

    // ═══════════════════════════════════════════════════════
    // SISTEMA DE GUARDADO Y CARGA
    // ═══════════════════════════════════════════════════════

    public void SaveGame()
    {
        PlayerPrefs.SetInt("HasTorso", hasTorso ? 1 : 0);
        PlayerPrefs.SetInt("HasLegs", hasLegs ? 1 : 0);
        PlayerPrefs.SetInt("HasArms", hasArms ? 1 : 0);
        PlayerPrefs.SetInt("CoinsCollected", coinsCollected);
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.SetFloat("Health", health);
        PlayerPrefs.SetFloat("MaxHealth", maxHealth);

        // Variables personalizadas
        PlayerPrefs.SetInt("CustomNumber", customNumber);
        PlayerPrefs.SetString("CustomText", customText);
        PlayerPrefs.SetInt("CustomFlag", customFlag ? 1 : 0);

        PlayerPrefs.Save();

        if (showDebugLogs) Debug.Log("💾 Juego guardado!");
    }

    public void LoadGame()
    {
        hasTorso = PlayerPrefs.GetInt("HasTorso", 0) == 1;
        hasLegs = PlayerPrefs.GetInt("HasLegs", 0) == 1;
        hasArms = PlayerPrefs.GetInt("HasArms", 0) == 1;
        coinsCollected = PlayerPrefs.GetInt("CoinsCollected", 0);
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        health = PlayerPrefs.GetFloat("Health", 100f);
        maxHealth = PlayerPrefs.GetFloat("MaxHealth", 100f);

        // Variables personalizadas
        customNumber = PlayerPrefs.GetInt("CustomNumber", 0);
        customText = PlayerPrefs.GetString("CustomText", "");
        customFlag = PlayerPrefs.GetInt("CustomFlag", 0) == 1;

        if (showDebugLogs)
        {
            Debug.Log($"📂 Juego cargado - Torso: {hasTorso}, Piernas: {hasLegs}, Brazos: {hasArms}");
            Debug.Log($"   Monedas: {coinsCollected}, Nivel: {currentLevel}, Salud: {health}");
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();

        hasTorso = false;
        hasLegs = false;
        hasArms = false;
        coinsCollected = 0;
        currentLevel = 1;
        health = 100f;
        maxHealth = 100f;
        customNumber = 0;
        customText = "";
        customFlag = false;

        if (showDebugLogs) Debug.Log("🔄 Progreso reseteado!");
    }

    // ═══════════════════════════════════════════════════════
    // MÉTODOS DE UTILIDAD
    // ═══════════════════════════════════════════════════════

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentLevel = scene.buildIndex;
        SaveGame();

        if (showDebugLogs)
            Debug.Log($"📍 Escena cargada: {scene.name} (Index: {scene.buildIndex})");
    }

    // Método para acceder desde otros scripts
    public void PrintProgress()
    {
        Debug.Log("═══════════════════════════════════════");
        Debug.Log("PROGRESO DE R.U.B.O.:");
        Debug.Log($"  🦴 Torso: {(hasTorso ? "✓" : "✗")}");
        Debug.Log($"  🦵 Piernas: {(hasLegs ? "✓" : "✗")}");
        Debug.Log($"  💪 Brazos: {(hasArms ? "✓" : "✗")}");
        Debug.Log($"  🪙 Monedas: {coinsCollected}");
        Debug.Log($"  📊 Nivel actual: {currentLevel}");
        Debug.Log($"  ❤️ Salud: {health}/{maxHealth}");
        Debug.Log($"  🔢 Custom Number: {customNumber}");
        Debug.Log($"  📝 Custom Text: {customText}");
        Debug.Log("═══════════════════════════════════════");
    }
}