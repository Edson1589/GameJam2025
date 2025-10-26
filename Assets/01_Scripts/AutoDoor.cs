using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDoor : MonoBehaviour
{
    [Header("Movimiento de la puerta")]
    [SerializeField] private Transform doorMesh;

    [SerializeField] private float openOffsetY = 3f;

    [SerializeField] private float moveSpeed = 4f;

    [Header("Detección del jugador")]
    [SerializeField] private string playerTag = "Player";

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool playerNear = false;

    private void Start()
    {
        if (doorMesh == null)
        {
            doorMesh = this.transform;
        }

        closedPos = doorMesh.position;
        openPos = closedPos + new Vector3(0f, openOffsetY, 0f);
    }

    private void Update()
    {
        Vector3 targetPos = playerNear ? openPos : closedPos;

        doorMesh.position = Vector3.Lerp(
            doorMesh.position,
            targetPos,
            Time.deltaTime * moveSpeed
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerNear = false;
        }
    }
}
