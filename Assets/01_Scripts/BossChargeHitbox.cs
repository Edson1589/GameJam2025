using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossChargeHitbox : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 localOffset;
    [SerializeField] private Vector3 localEuler;

    [Header("Daño")]
    [SerializeField] private int damage = 3;

    void LateUpdate()
    {
        if (!target) return;

        transform.position = target.TransformPoint(localOffset);
        transform.rotation = target.rotation * Quaternion.Euler(localEuler);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }
}
