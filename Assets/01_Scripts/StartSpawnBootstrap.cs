using UnityEngine;

public class StartSpawnBootstrap : MonoBehaviour
{
    [SerializeField] private Transform initialSpawn;

    void Start()
    {
        if (initialSpawn)
            ZoneSpawnManager.Instance?.SetSpawnPoint(initialSpawn);
    }
}
