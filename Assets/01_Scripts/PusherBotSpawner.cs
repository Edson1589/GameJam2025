using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PusherBotSpawner : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PusherBot pusherPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private Collider spawnArea;
    [SerializeField] private BossHealth bossHealth;

    [Header("Spawn continuo")]
    [SerializeField] private float spawnEverySeconds = 2.0f;
    [SerializeField] private int maxAlive = 6;
    [SerializeField] private float botLifetime = 6f;

    [Header("Altura de vuelo")]
    [SerializeField] private bool randomizeY = true;
    [SerializeField] private float yFixed = 1.5f;
    [SerializeField] private Vector2 yRange = new Vector2(1.0f, 3.0f);

    private readonly HashSet<PusherBot> alive = new HashSet<PusherBot>();
    private Coroutine loop;
    private bool running = false;

    public void BeginContinuous()
    {
        if (running) return;
        running = true;
        loop = StartCoroutine(SpawnLoop());
    }

    public void StopContinuous(bool clearBots = true)
    {
        running = false;
        if (loop != null) { StopCoroutine(loop); loop = null; }
        if (clearBots)
        {
            foreach (var b in alive) if (b) Destroy(b.gameObject);
            alive.Clear();
        }
    }

    public void OnBotDestroyed(PusherBot bot) { alive.Remove(bot); }

    private IEnumerator SpawnLoop()
    {
        while (running)
        {
            bool bossAlive = bossHealth != null && bossHealth.CurrentHP > 0;
            if (!bossAlive)
            {
                StopContinuous(clearBots: true);
                yield break;
            }

            if (pusherPrefab && player && spawnArea && alive.Count < maxAlive)
            {
                Vector3 pos = GetRandomPointOnPlane(spawnArea);
                var bot = Instantiate(pusherPrefab, pos, Quaternion.identity);
                bot.Init(player, botLifetime, this);
                alive.Add(bot);
            }

            yield return new WaitForSeconds(spawnEverySeconds);
        }
    }

    private Vector3 GetRandomPointOnPlane(Collider area)
    {
        Bounds b = area.bounds;
        float x = Random.Range(b.min.x, b.max.x);
        float z = Random.Range(b.min.z, b.max.z);

        float y = randomizeY ? Random.Range(yRange.x, yRange.y) : yFixed;

        y += b.center.y;

        return new Vector3(x, y, z);
    }
}
