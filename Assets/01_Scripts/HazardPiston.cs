using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardPiston : MonoBehaviour
{
    [Header("Parte que se mueve (el bloque que aplasta)")]
    [SerializeField] private Transform pistonHead;

    [Header("Offset cuando está extendido")]
    [SerializeField] private Vector3 extendedOffset = new Vector3(0f, -2f, 0f);

    [Header("Velocidad de movimiento")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Tiempos de pausa")]
    [SerializeField] private float holdExtendedTime = 1.5f;
    [SerializeField] private float holdRetractedTime = 1.5f;

    private Vector3 retractedPos;
    private Vector3 extendedPos;

    private void Start()
    {
        if (pistonHead == null)
        {
            pistonHead = this.transform;
        }

        retractedPos = pistonHead.position;
        extendedPos = retractedPos + extendedOffset;

        StartCoroutine(PistonLoop());
    }

    private IEnumerator PistonLoop()
    {
        while (true)
        {
            yield return StartCoroutine(MovePart(pistonHead, extendedPos));
            yield return new WaitForSeconds(holdExtendedTime);
            yield return StartCoroutine(MovePart(pistonHead, retractedPos));
            yield return new WaitForSeconds(holdRetractedTime);
        }
    }

    private IEnumerator MovePart(Transform t, Vector3 dest)
    {
        while (Vector3.Distance(t.position, dest) > 0.01f)
        {
            t.position = Vector3.MoveTowards(
                t.position,
                dest,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
        t.position = dest;
    }
}
