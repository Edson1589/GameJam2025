using UnityEngine;

public class BodyPartsVisualizer : MonoBehaviour
{
    [Header("Body Parts Prefabs/Objects")]
    [SerializeField] private GameObject legsPrefab;
    [SerializeField] private GameObject leftArmPrefab;
    [SerializeField] private GameObject rightArmPrefab;
    [SerializeField] private GameObject torsoPrefab;

    [Header("Attachment Points")]
    [SerializeField] private Transform legsAttachPoint;
    [SerializeField] private Transform leftArmAttachPoint;
    [SerializeField] private Transform rightArmAttachPoint;
    [SerializeField] private Transform torsoAttachPoint;

    [Header("Visual Settings")]
    [SerializeField] private float attachAnimationSpeed = 5f;
    private GameObject legsInstance;
    private GameObject leftArmInstance;
    private GameObject rightArmInstance;
    private GameObject torsoInstance;

    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();

        // Crear los attachment points si no existen
        CreateAttachmentPoints();
        Debug.Log("=== VERIFICACION DE PREFABS ===");
        Debug.Log("Legs Prefab: " + (legsPrefab != null ? legsPrefab.name : "NULL"));
        Debug.Log("Left Arm Prefab: " + (leftArmPrefab != null ? leftArmPrefab.name : "NULL"));
        Debug.Log("Right Arm Prefab: " + (rightArmPrefab != null ? rightArmPrefab.name : "NULL"));
        Debug.Log("Torso Prefab: " + (torsoPrefab != null ? torsoPrefab.name : "NULL"));
        Debug.Log("================================");
    }

    private void CreateAttachmentPoints()
    {
        if (legsAttachPoint == null)
        {
            GameObject legsPoint = new GameObject("LegsAttachPoint");
            legsPoint.transform.SetParent(transform);
            legsPoint.transform.localPosition = new Vector3(0, -0.6f, 0);
            legsAttachPoint = legsPoint.transform;
            Debug.Log("LegsAttachPoint creado automaticamente");
        }

        if (leftArmAttachPoint == null)
        {
            GameObject leftPoint = new GameObject("LeftArmAttachPoint");
            leftPoint.transform.SetParent(transform);
            leftPoint.transform.localPosition = new Vector3(-0.7f, 0.2f, 0);
            leftArmAttachPoint = leftPoint.transform;
            Debug.Log("LeftArmAttachPoint creado automaticamente");
        }

        if (rightArmAttachPoint == null)
        {
            GameObject rightPoint = new GameObject("RightArmAttachPoint");
            rightPoint.transform.SetParent(transform);
            rightPoint.transform.localPosition = new Vector3(0.7f, 0.2f, 0);
            rightArmAttachPoint = rightPoint.transform;
            Debug.Log("RightArmAttachPoint creado automaticamente");
        }

        if (torsoAttachPoint == null)
        {
            GameObject torsoPoint = new GameObject("TorsoAttachPoint");
            torsoPoint.transform.SetParent(transform);
            torsoPoint.transform.localPosition = new Vector3(0, -0.2f, 0);
            torsoAttachPoint = torsoPoint.transform;
            Debug.Log("TorsoAttachPoint creado automaticamente");
        }
    }

    public void AttachLegs()
    {
        Debug.Log(">>> AttachLegs() INICIADO");

        if (legsPrefab == null)
        {
            Debug.LogError(">>> ERROR: legsPrefab es NULL! Asignalo en el Inspector del Player");
            return;
        }

        if (legsInstance != null)
        {
            Debug.LogWarning(">>> Las piernas ya estan acopladas");
            return;
        }

        if (legsAttachPoint == null)
        {
            Debug.LogError(">>> ERROR: legsAttachPoint es NULL!");
            return;
        }

        Debug.Log(">>> Instanciando prefab de piernas: " + legsPrefab.name);
        legsInstance = Instantiate(legsPrefab, legsAttachPoint);
        legsInstance.transform.localPosition = Vector3.zero;
        legsInstance.transform.localRotation = Quaternion.identity;

        Debug.Log(">>> Piernas instanciadas exitosamente!");
        Debug.Log(">>> Posicion mundial: " + legsInstance.transform.position);
        Debug.Log(">>> Posicion local: " + legsInstance.transform.localPosition);

        StartCoroutine(AttachAnimation(legsInstance));

        Debug.Log("Piernas acopladas visualmente!");
    }

    public void AttachArms()
    {
        Debug.Log(">>> AttachArms() INICIADO");

        if (leftArmPrefab == null || rightArmPrefab == null)
        {
            Debug.LogError(">>> ERROR: Prefabs de brazos son NULL! Asignalos en el Inspector");
            return;
        }

        if (leftArmInstance == null)
        {
            Debug.Log(">>> Instanciando brazo izquierdo");
            leftArmInstance = Instantiate(leftArmPrefab, leftArmAttachPoint);
            leftArmInstance.transform.localPosition = Vector3.zero;
            leftArmInstance.transform.localRotation = Quaternion.identity;
            StartCoroutine(AttachAnimation(leftArmInstance));
        }

        if (rightArmInstance == null)
        {
            Debug.Log(">>> Instanciando brazo derecho");
            rightArmInstance = Instantiate(rightArmPrefab, rightArmAttachPoint);
            rightArmInstance.transform.localPosition = Vector3.zero;
            rightArmInstance.transform.localRotation = Quaternion.identity;
            StartCoroutine(AttachAnimation(rightArmInstance));
        }

        Debug.Log("Brazos acoplados visualmente!");
    }

    public void AttachTorso()
    {
        Debug.Log(">>> AttachTorso() INICIADO");

        if (torsoPrefab == null)
        {
            Debug.LogError(">>> ERROR: torsoPrefab es NULL! Asignalo en el Inspector");
            return;
        }

        if (torsoInstance != null)
        {
            Debug.LogWarning(">>> El torso ya esta acoplado");
            return;
        }

        Debug.Log(">>> Instanciando prefab de torso");
        torsoInstance = Instantiate(torsoPrefab, torsoAttachPoint);
        torsoInstance.transform.localPosition = Vector3.zero;
        torsoInstance.transform.localRotation = Quaternion.identity;
        StartCoroutine(AttachAnimation(torsoInstance));

        Debug.Log("Torso acoplado visualmente!");
    }

    private System.Collections.IEnumerator AttachAnimation(GameObject part)
    {
        if (part == null)
        {
            Debug.LogWarning(">>> AttachAnimation recibio un objeto NULL");
            yield break;
        }

        Vector3 targetScale = part.transform.localScale;
        part.transform.localScale = Vector3.zero;

        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            float bounce = Mathf.Sin(progress * Mathf.PI);
            part.transform.localScale = targetScale * Mathf.Lerp(0, 1.2f, progress) * (1 + bounce * 0.2f);

            yield return null;
        }

        part.transform.localScale = targetScale;
        Debug.Log(">>> Animacion de acoplamiento completada para: " + part.name);
    }

    public bool HasLegsAttached() => legsInstance != null;
    public bool HasArmsAttached() => leftArmInstance != null && rightArmInstance != null;
    public bool HasTorsoAttached() => torsoInstance != null;
}