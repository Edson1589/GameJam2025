using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DroneChaserSimple : MonoBehaviour
{
    [Header("Objetivo a perseguir")]
    public Transform targetPlayer;

    [Header("Refs")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform droneVisual;
    [SerializeField] private float hoverHeightOffset = 1.0f;

    [Header("Hover bob (solo visual)")]
    [SerializeField] private float bobAmp = 0.1f;
    [SerializeField] private float bobFreq = 4f;

    [Header("Estado")]
    public bool chaseActive = false;

    private void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.updateRotation = true;
            agent.updateUpAxis = true;
        }
    }

    private void Update()
    {
        if (!chaseActive) return;
        if (targetPlayer == null) return;
        if (agent == null) return;

        agent.SetDestination(targetPlayer.position);

        if (droneVisual != null)
        {
            Vector3 basePos = transform.position;
            basePos.y += hoverHeightOffset;
            basePos.y += Mathf.Sin(Time.time * bobFreq) * bobAmp;

            droneVisual.position = basePos;

            Vector3 lookDir = targetPlayer.position - droneVisual.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion rot = Quaternion.LookRotation(lookDir, Vector3.up);
                droneVisual.rotation = Quaternion.Slerp(droneVisual.rotation, rot, 15f * Time.deltaTime);
            }
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

            Debug.Log("El dron atrapó al jugador -> respawn.");
        }
    }
}
