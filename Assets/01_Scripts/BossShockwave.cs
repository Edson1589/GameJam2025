using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossShockwave : MonoBehaviour
{
    [Header("Jugador (para checar daño)")]
    [SerializeField] private Transform player;
    [SerializeField] private float killYTolerance = 1.0f;

    [Header("Daño")]
    [SerializeField] private int damage = 2;

    [Header("Crecimiento de la onda")]
    [SerializeField] private float startRadius = 0.5f;
    [SerializeField] private float expandSpeed = 10f;
    [SerializeField] private float maxRadius = 15f;
    [SerializeField] private float lifeTime = 2.0f;

    [Header("Visual")]
    [SerializeField] private Transform waveVisual;
    [SerializeField] private float waveHeightY = 0.05f;
    [SerializeField] private float visualHeightScale = 0.05f;

    private float currentRadius;
    private float prevRadius;
    private float timer;
    private bool hasHitPlayer = false;

    public void Init(Transform playerRef)
    {
        player = playerRef;
        currentRadius = startRadius;
        prevRadius = startRadius;
        timer = lifeTime;
        hasHitPlayer = false;

        UpdateVisual();
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f || currentRadius >= maxRadius)
        {
            Destroy(gameObject);
            return;
        }

        prevRadius = currentRadius;
        currentRadius += expandSpeed * Time.deltaTime;

        UpdateVisual();
        CheckPlayerDamageOnce();
    }

    private void UpdateVisual()
    {
        if (waveVisual == null) return;

        float diameter = currentRadius * 2f;

        waveVisual.localScale = new Vector3(
            diameter,
            visualHeightScale,
            diameter
        );

        Vector3 basePos = transform.position;
        basePos.y = waveHeightY;
        waveVisual.position = basePos;
    }

    private void CheckPlayerDamageOnce()
    {
        if (player == null || hasHitPlayer) return;

        float heightDiff = Mathf.Abs(player.position.y - waveHeightY);
        if (heightDiff > killYTolerance) return;

        Vector3 center = transform.position;
        Vector2 centerXZ = new Vector2(center.x, center.z);
        Vector2 playerXZ = new Vector2(player.position.x, player.position.z);
        float dist = Vector2.Distance(playerXZ, centerXZ);

        bool ringCross = (prevRadius <= dist && dist <= currentRadius);

        if (ringCross)
        {
            var health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                hasHitPlayer = true;
            }
        }
    }
}
