using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegCapsuleController : MonoBehaviour
{
    [Header("Refs básicas")]
    [SerializeField] private Transform capsuleRig;
    [SerializeField] private Transform groundTarget;

    [Header("Puertas")]
    [SerializeField] private Transform doorLeft;
    [SerializeField] private Transform doorRight;

    [Header("Puerta - Rotaciones")]
    [SerializeField] private Vector3 doorLeftOpenEuler = new Vector3(0f, -90f, 0f);
    [SerializeField] private Vector3 doorRightOpenEuler = new Vector3(0f, 90f, 0f);

    private Quaternion doorLeftClosedRot;
    private Quaternion doorRightClosedRot;
    private Quaternion doorLeftOpenRot;
    private Quaternion doorRightOpenRot;

    [Header("Contenido interno")]
    [SerializeField] private GameObject legsPickupObject;

    [Header("Velocidades / tiempos")]
    [SerializeField] private float descendSpeed = 3f;
    [SerializeField] private float doorOpenSpeed = 4f;
    [SerializeField] private float pauseBeforeOpen = 0.5f;

    [Header("Control del jugador durante la cinemática")]
    [SerializeField] private MonoBehaviour[] playerControlScripts;
    [SerializeField] private Transform playerRef;
    [SerializeField] private bool lockPlayerDuringSequence = true;

    private bool sequenceStarted = false;
    private bool sequenceFinished = false;

    private void Start()
    {
        if (capsuleRig == null)
            capsuleRig = this.transform;

        if (doorLeft != null) doorLeftClosedRot = doorLeft.localRotation;
        if (doorRight != null) doorRightClosedRot = doorRight.localRotation;

        doorLeftOpenRot = Quaternion.Euler(doorLeftOpenEuler);
        doorRightOpenRot = Quaternion.Euler(doorRightOpenEuler);

        if (legsPickupObject != null)
            legsPickupObject.SetActive(false);
    }

    public void BeginSequence()
    {
        if (sequenceStarted) return;
        sequenceStarted = true;
        StartCoroutine(CapsuleSequence());
    }

    private IEnumerator CapsuleSequence()
    {
        if (lockPlayerDuringSequence)
            SetPlayerControlEnabled(false);

        yield return StartCoroutine(DescendCapsule());

        yield return new WaitForSeconds(pauseBeforeOpen);

        yield return StartCoroutine(OpenDoors());

        if (legsPickupObject != null)
            legsPickupObject.SetActive(true);

        if (lockPlayerDuringSequence)
            SetPlayerControlEnabled(true);

        sequenceFinished = true;
        Debug.Log("Capsula de Piernas lista. Piernas disponibles.");
    }

    private IEnumerator DescendCapsule()
    {
        if (groundTarget == null)
        {
            Debug.LogWarning("No hay groundTarget asignado en la cápsula de piernas.");
            yield break;
        }

        while (Vector3.Distance(capsuleRig.position, groundTarget.position) > 0.01f)
        {
            capsuleRig.position = Vector3.MoveTowards(
                capsuleRig.position,
                groundTarget.position,
                descendSpeed * Time.deltaTime
            );

            if (playerRef != null)
            {
                Vector3 lookAt = new Vector3(playerRef.position.x, capsuleRig.position.y, playerRef.position.z);
                capsuleRig.rotation = Quaternion.Slerp(
                    capsuleRig.rotation,
                    Quaternion.LookRotation(lookAt - capsuleRig.position),
                    5f * Time.deltaTime
                );
            }

            yield return null;
        }

        capsuleRig.position = groundTarget.position;
    }

    private IEnumerator OpenDoors()
    {
        if (doorLeft == null || doorRight == null)
        {
            Debug.LogWarning("No hay puertas asignadas en la cápsula de piernas.");
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * doorOpenSpeed;

            doorLeft.localRotation = Quaternion.Slerp(doorLeftClosedRot, doorLeftOpenRot, t);
            doorRight.localRotation = Quaternion.Slerp(doorRightClosedRot, doorRightOpenRot, t);

            yield return null;
        }

        doorLeft.localRotation = doorLeftOpenRot;
        doorRight.localRotation = doorRightOpenRot;
    }

    private void SetPlayerControlEnabled(bool enabled)
    {
        foreach (var scr in playerControlScripts)
        {
            if (scr != null)
                scr.enabled = enabled;
        }
    }
}
