using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossChargeHitbox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRespawnHandler resp = other.GetComponent<PlayerRespawnHandler>();
            if (resp != null)
            {
                resp.RespawnNow();
            }
        }
    }
}
