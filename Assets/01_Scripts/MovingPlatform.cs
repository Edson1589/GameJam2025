using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Puntos de movimiento")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Velocidad")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Pausa al llegar (segundos)")]
    [SerializeField] private float waitTime = 1f;

    private Transform currentTarget;
    private bool isWaiting = false;

    private void Start()
    {
        currentTarget = pointB;
    }

    private void Update()
    {
        if (isWaiting || pointA == null || pointB == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, currentTarget.position) < 0.01f)
        {
            StartCoroutine(SwitchTargetAfterDelay());
        }
    }

    private IEnumerator SwitchTargetAfterDelay()
    {
        isWaiting = true;

        yield return new WaitForSeconds(waitTime);

        if (currentTarget == pointB)
        {
            currentTarget = pointA;
        }
        else
        {
            currentTarget = pointB;
        }

        isWaiting = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(this.transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.position, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pointB.position, 0.1f);
        }
    }
}
