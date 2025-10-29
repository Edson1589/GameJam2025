using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 4f;
    private Vector3 velocity;

    public void Init(Vector3 dir, float speed)
    {
        velocity = dir.normalized * speed;
    }

    private void Update()
    {
        transform.position += velocity * Time.deltaTime;

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRespawnHandler resp = other.GetComponent<PlayerRespawnHandler>();
            if (resp != null)
            {
                resp.RespawnNow();
            }

            Destroy(gameObject);
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            Destroy(gameObject);
        }
    }
}
