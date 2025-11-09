using UnityEngine;

public class ZoneSpawnManager : MonoBehaviour
{
    public static ZoneSpawnManager Instance { get; private set; }

    [SerializeField] private Transform currentSpawn;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetSpawnPoint(Transform spawn)
    {
        if (!spawn) return;
        currentSpawn = spawn;
    }

    public Transform GetSpawnPoint() => currentSpawn;
}
