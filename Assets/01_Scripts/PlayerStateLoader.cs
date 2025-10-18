using UnityEngine;

/// <summary>
/// Carga el estado guardado del jugador R.U.B.O. cuando inicia una escena
/// Agrégalo al objeto del jugador junto con PlayerController
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerStateLoader : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool showDebugLogs = true;

    private PlayerController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        if (loadOnStart)
        {
            LoadPlayerState();
        }
    }

    public void LoadPlayerState()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("⚠️ GameManager no encontrado! Crea un objeto con GameManager.cs en la primera escena.");
            return;
        }

        if (playerController == null)
        {
            Debug.LogError("⚠️ PlayerController no encontrado!");
            return;
        }

        // Sincronizar estado desde GameManager al PlayerController
        playerController.hasTorso = GameManager.Instance.hasTorso;
        playerController.hasLegs = GameManager.Instance.hasLegs;
        playerController.hasArms = GameManager.Instance.hasArms;

        // Aplicar las partes del cuerpo si ya las tiene
        if (GameManager.Instance.hasTorso)
        {
            playerController.ConnectTorso();
        }

        if (GameManager.Instance.hasLegs)
        {
            playerController.ConnectLegs();
        }

        if (GameManager.Instance.hasArms)
        {
            playerController.ConnectArms();
        }

        if (showDebugLogs)
        {
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("R.U.B.O. Estado cargado:");
            Debug.Log($"  🦴 Torso: {(GameManager.Instance.hasTorso ? "✓ CONECTADO" : "✗ Desconectado")}");
            Debug.Log($"  🦵 Piernas: {(GameManager.Instance.hasLegs ? "✓ CONECTADAS" : "✗ Desconectadas")}");
            Debug.Log($"  💪 Brazos: {(GameManager.Instance.hasArms ? "✓ CONECTADOS" : "✗ Desconectados")}");
            Debug.Log($"  🪙 Monedas: {GameManager.Instance.coinsCollected}");
            Debug.Log($"  🔢 Custom Number: {GameManager.Instance.customNumber}");
            Debug.Log("═══════════════════════════════════════");
        }
    }

    // Método para cuando R.U.B.O. encuentra una parte nueva
    public void OnPartCollected(string partType)
    {
        if (GameManager.Instance == null) return;

        switch (partType.ToLower())
        {
            case "torso":
                GameManager.Instance.UnlockTorso();
                playerController.ConnectTorso();
                break;

            case "legs":
            case "piernas":
                GameManager.Instance.UnlockLegs();
                playerController.ConnectLegs();
                break;

            case "arms":
            case "brazos":
                GameManager.Instance.UnlockArms();
                playerController.ConnectArms();
                break;

            default:
                Debug.LogWarning($"Tipo de parte desconocido: {partType}");
                break;
        }

        // Guardar inmediatamente después de recoger una parte
        GameManager.Instance.SaveGame();
    }
}