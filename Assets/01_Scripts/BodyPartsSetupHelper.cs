using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script helper que organiza automáticamente las partes del cuerpo de RUBO
/// Adjúntalo temporalmente a RUBO_Player y ejecútalo desde el Inspector
/// </summary>
public class BodyPartsSetupHelper : MonoBehaviour
{
    [Header("Referencias del Modelo")]
    [SerializeField] private Transform ruboArmature; 

    [Header("Nombres de las Partes")]
    [SerializeField] private string headName = "RUBO_Head";
    [SerializeField] private string torsoName = "RUBO_Torso";
    [SerializeField]
    private string[] armNames = new string[]
    {
        "RUBO_RightUpperArm", "RUBO_RightForearm", "RUBO_RightHand",
        "RUBO_LeftUpperArm", "RUBO_LeftForearm", "RUBO_LeftHand"
    };
    [SerializeField]
    private string[] legNames = new string[]
    {
        "RUBO_LeftUpperLeg", "RUBO_LeftCalf", "RUBO_LeftFoot",
        "RUBO_RightUpperLeg", "RUBO_RightCalf", "RUBO_RightFoot"
    };

    [Header("Estado Inicial")]
    [SerializeField] private bool startWithHead = true;
    [SerializeField] private bool startWithTorso = false;
    [SerializeField] private bool startWithArms = false;
    [SerializeField] private bool startWithLegs = false;

    [Header("Configuración del Collider")]
    [SerializeField] private float headOnlyColliderHeight = 0.5f;
    [SerializeField] private Vector3 headOnlyColliderCenter = new Vector3(0, 0.25f, 0);
    [SerializeField] private float colliderRadius = 0.25f;

#if UNITY_EDITOR
    [ContextMenu("1. Organizar Partes del Cuerpo")]
    public void OrganizeBodyParts()
    {
        if (ruboArmature == null)
        {
            Debug.LogError("¡Asigna el Armature RUBO primero!");
            return;
        }

        // Crear grupos
        GameObject torsoGroup = CreateOrGetGroup("TorsoGroup");
        GameObject armsGroup = CreateOrGetGroup("ArmsGroup");
        GameObject legsGroup = CreateOrGetGroup("LegsGroup");
        GameObject headGroup = CreateOrGetGroup("HeadGroup");

        // Organizar torso
        Transform torsoTransform = FindChildRecursive(ruboArmature, torsoName);
        if (torsoTransform != null)
        {
            torsoTransform.SetParent(torsoGroup.transform);
            Debug.Log("✓ Torso organizado");
        }

        // Organizar brazos
        int armsFound = 0;
        foreach (string armName in armNames)
        {
            Transform armTransform = FindChildRecursive(ruboArmature, armName);
            if (armTransform != null)
            {
                armTransform.SetParent(armsGroup.transform);
                armsFound++;
            }
        }
        Debug.Log($"✓ {armsFound} partes de brazos organizadas");

        // Organizar piernas
        int legsFound = 0;
        foreach (string legName in legNames)
        {
            Transform legTransform = FindChildRecursive(ruboArmature, legName);
            if (legTransform != null)
            {
                legTransform.SetParent(legsGroup.transform);
                legsFound++;
            }
        }
        Debug.Log($"✓ {legsFound} partes de piernas organizadas");

        // Organizar cabeza
        Transform headTransform = FindChildRecursive(ruboArmature, headName);
        if (headTransform != null)
        {
            headTransform.SetParent(headGroup.transform);
            Debug.Log("✓ Cabeza organizada");
        }

        // Asignar referencias al PlayerController
        AssignToPlayerController(headGroup, torsoGroup, armsGroup, legsGroup);

        Debug.Log("✅ Organización completa!");
    }

    [ContextMenu("2. Configurar Estado Inicial")]
    public void SetInitialState()
    {
        Transform headGroup = transform.Find("Model/RUBO/HeadGroup");
        Transform torsoGroup = transform.Find("Model/RUBO/TorsoGroup");
        Transform armsGroup = transform.Find("Model/RUBO/ArmsGroup");
        Transform legsGroup = transform.Find("Model/RUBO/LegsGroup");

        if (headGroup) headGroup.gameObject.SetActive(startWithHead);
        if (torsoGroup) torsoGroup.gameObject.SetActive(startWithTorso);
        if (armsGroup) armsGroup.gameObject.SetActive(startWithArms);
        if (legsGroup) legsGroup.gameObject.SetActive(startWithLegs);

        Debug.Log($"✅ Estado inicial configurado: Head={startWithHead}, Torso={startWithTorso}, Arms={startWithArms}, Legs={startWithLegs}");
    }

    [ContextMenu("3. Ajustar Collider Inicial")]
    public void AdjustInitialCollider()
    {
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            capsule.height = headOnlyColliderHeight;
            capsule.center = headOnlyColliderCenter;
            capsule.radius = colliderRadius;
            capsule.direction = 1; // Y-axis

            Debug.Log($"✅ Collider ajustado: Height={headOnlyColliderHeight}, Center={headOnlyColliderCenter}, Radius={colliderRadius}");
        }
        else
        {
            Debug.LogError("No se encontró CapsuleCollider!");
        }
    }

    [ContextMenu("4. EJECUTAR TODO")]
    public void SetupEverything()
    {
        OrganizeBodyParts();
        SetInitialState();
        AdjustInitialCollider();
        Debug.Log("🎉 ¡Setup completo! Ahora desactiva este componente.");
    }

    private GameObject CreateOrGetGroup(string groupName)
    {
        Transform existing = ruboArmature.Find(groupName);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject newGroup = new GameObject(groupName);
        newGroup.transform.SetParent(ruboArmature);
        newGroup.transform.localPosition = Vector3.zero;
        newGroup.transform.localRotation = Quaternion.identity;
        newGroup.transform.localScale = Vector3.one;

        return newGroup;
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform found = FindChildRecursive(child, childName);
            if (found != null)
                return found;
        }
        return null;
    }

    private void AssignToPlayerController(GameObject head, GameObject torso, GameObject arms, GameObject legs)
    {
        PlayerController player = GetComponent<PlayerController>();
        if (player != null)
        {
            SerializedObject so = new SerializedObject(player);

            so.FindProperty("headGO").objectReferenceValue = head;
            so.FindProperty("torsoGroup").objectReferenceValue = torso;
            so.FindProperty("armsGroup").objectReferenceValue = arms;
            so.FindProperty("legsGroup").objectReferenceValue = legs;

            so.ApplyModifiedProperties();

            Debug.Log("✓ Referencias asignadas al PlayerController");
        }
    }
#endif
}