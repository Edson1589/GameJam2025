using UnityEngine;

public class PartAnimator : MonoBehaviour
{
    [Tooltip("El Transform del HUESO en el Rig animado de Mixamo que esta parte debe seguir.")]
    public Transform targetBone;

    // Nueva variable para ajustar la rotación
    [Tooltip("Ajuste manual para corregir la rotación (ej: Quaternion.Euler(0, 180, 0))")]
    public Quaternion rotationOffset = Quaternion.identity; // Usa Quaternion.identity por defecto

    private Transform thisTransform;

    void Start()
    {
        thisTransform = transform;
    }

    void LateUpdate()
    {
        if (targetBone != null)
        {
            // Aplica la rotación del hueso Y el ajuste de rotación (offset)
            thisTransform.localRotation = targetBone.localRotation * rotationOffset;
        }
    }
}