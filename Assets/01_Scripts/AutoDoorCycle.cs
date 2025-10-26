using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDoorCycle : MonoBehaviour
{
    [Header("Objeto que se mueve como puerta")]
    [SerializeField] private Transform doorMesh;

    [Header("Apertura (offset desde la posición cerrada)")]
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 3f, 0f);

    [Header("Velocidad de movimiento")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Tiempos de espera")]
    [SerializeField] private float stayOpenTime = 2f;
    [SerializeField] private float stayClosedTime = 2f;

    private Vector3 closedPos;
    private Vector3 openPos;

    private void Start()
    {
        if (doorMesh == null)
        {
            doorMesh = this.transform;
        }

        closedPos = doorMesh.position;
        openPos = closedPos + openOffset;

        StartCoroutine(DoorLoop());
    }

    private IEnumerator DoorLoop()
    {
        while (true)
        {
            yield return StartCoroutine(MoveDoor(doorMesh, openPos));

            yield return new WaitForSeconds(stayOpenTime);

            yield return StartCoroutine(MoveDoor(doorMesh, closedPos));

            yield return new WaitForSeconds(stayClosedTime);
        }
    }

    private IEnumerator MoveDoor(Transform target, Vector3 dest)
    {
        while (Vector3.Distance(target.position, dest) > 0.01f)
        {
            target.position = Vector3.MoveTowards(
                target.position,
                dest,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        target.position = dest;
    }
}
