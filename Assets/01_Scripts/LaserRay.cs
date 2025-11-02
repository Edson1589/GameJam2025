using UnityEngine;
using System.Collections;

public class LaserRay : MonoBehaviour
{
    public enum VisualMode { LineRenderer, Cylinder }

    [Header("Refs")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform camTransform;

    [Header("Raycast")]
    [SerializeField] private float range = 60f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private QueryTriggerInteraction triggerMode = QueryTriggerInteraction.Collide;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 0.30f;
    private float nextFireTime = 0f;

    [Header("Unlock / Requisitos")]
    [SerializeField] private bool requiresTorso = true;
    [SerializeField] private bool unlocked = false;

    [Header("Visual")]
    [SerializeField] private VisualMode visualMode = VisualMode.LineRenderer;
    [SerializeField] private float beamDuration = 0.05f;

    [SerializeField] private LineRenderer line;
    [SerializeField] private GameObject cylinderBeam;

    [Header("Beam (cylinder) — ajustes finos")]
    [SerializeField] private Vector3 cylinderAxis = Vector3.up;
    [SerializeField] private bool detachBeamFromParent = true;
    [SerializeField] private float cylinderDiameter = 0.12f;
    [SerializeField] private float cylinderExtraLength = 0f;


    private void Awake()
    {
        if (!camTransform && Camera.main) camTransform = Camera.main.transform;
        if (line) line.enabled = false;

        if (cylinderBeam)
        {
            var col = cylinderBeam.GetComponent<Collider>();
            if (col) Destroy(col);
            cylinderBeam.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryFire();
    }

    public void SetUnlocked(bool on) => unlocked = on;
    public bool IsUnlocked => unlocked;

    public bool IsReady() => Time.time >= nextFireTime;

    public float Cooldown01()
    {
        if (cooldown <= 0f) return 1f;
        float remaining = nextFireTime - Time.time;
        return Mathf.Clamp01(1f - Mathf.Clamp01(remaining / cooldown));
    }

    public void TryFire()
    {
        if (!IsReady()) return;
        if (requiresTorso && !unlocked) return;
        Fire();
        nextFireTime = Time.time + cooldown;
    }

    public void Fire()
    {
        Transform originTf = firePoint ? firePoint : transform;
        Vector3 origin = originTf.position;
        Vector3 dir = (camTransform ? camTransform.forward : transform.forward).normalized;

        Vector3 acceptedEnd = origin + dir * range;

        var hits = Physics.RaycastAll(origin, dir, range, hitMask, triggerMode);
        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var h in hits)
            {
                if (h.collider.transform.root == transform.root) continue;

                var bot = h.collider.GetComponent<PusherBot>() ?? h.collider.GetComponentInParent<PusherBot>();
                if (bot != null)
                {
                    acceptedEnd = h.point;
                    bot.KillByLaser();
                    break;
                }

                if (!h.collider.isTrigger)
                {
                    acceptedEnd = h.point;
                    break;
                }
            }
        }

        if (visualMode == VisualMode.LineRenderer && line)
            StartCoroutine(ShowBeamLine(origin, acceptedEnd));
        else if (visualMode == VisualMode.Cylinder && cylinderBeam)
            StartCoroutine(ShowBeamCylinder(origin, acceptedEnd, dir));
    }


    private IEnumerator ShowBeamLine(Vector3 a, Vector3 b)
    {
        line.positionCount = 2;
        line.SetPosition(0, a);
        line.SetPosition(1, b);
        line.enabled = true;
        yield return new WaitForSeconds(beamDuration);
        line.enabled = false;
    }

    private IEnumerator ShowBeamCylinder(Vector3 origin, Vector3 endPoint, Vector3 dir)
    {
        float length = Vector3.Distance(origin, endPoint) + cylinderExtraLength;
        Vector3 mid = origin + dir * (length * 0.5f);

        if (detachBeamFromParent && cylinderBeam.transform.parent != null)
            cylinderBeam.transform.SetParent(null, true);

        cylinderBeam.SetActive(true);

        Vector3 s = cylinderBeam.transform.localScale;
        s.x = cylinderDiameter;
        s.z = cylinderDiameter;
        s.y = length * 0.5f;
        cylinderBeam.transform.localScale = s;

        cylinderBeam.transform.rotation = Quaternion.FromToRotation(cylinderAxis, dir);

        cylinderBeam.transform.position = mid;

        yield return new WaitForSeconds(beamDuration);
        cylinderBeam.SetActive(false);
    }
}
