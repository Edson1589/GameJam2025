using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Orbit & Zoom")]
    [SerializeField] private float distance = 4f;
    [SerializeField] private float minDistance = 1.2f;
    [SerializeField] private float maxDistance = 6f;
    [SerializeField] private float mouseSensitivity = 120f;
    [SerializeField] private float pitchMin = -25f;
    [SerializeField] private float pitchMax = 70f;
    [SerializeField] private bool invertY = false;

    [Header("Suavizado & Colisión")]
    [SerializeField] private float positionSmooth = 0.05f;
    [SerializeField] private float collisionRadius = 0.25f;
    [SerializeField] private LayerMask obstacleMask = ~0;

    float yaw, pitch;
    Vector3 vel;

    void Start()
    {
        if (!target) Debug.LogWarning("ThirdPersonCamera: asigna 'target'.");
        yaw = target ? target.eulerAngles.y : 0f;
        pitch = 10f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!target) return;

        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        yaw += mx * mouseSensitivity * Time.deltaTime;
        pitch += (invertY ? my : -my) * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        // Colisión cámara
        Vector3 pivot = target.position + targetOffset;
        Vector3 dir = rot * Vector3.back;
        float want = distance;

        if (Physics.SphereCast(pivot, collisionRadius, dir, out RaycastHit hit,
                               distance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            want = Mathf.Clamp(hit.distance - 0.05f, minDistance, distance);
        }

        Vector3 desired = pivot + dir * want;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref vel, positionSmooth);
        transform.rotation = rot;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
            distance = Mathf.Clamp(distance - scroll * 2.5f, minDistance, maxDistance);
    }
}
