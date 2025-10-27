using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class UIParticleHelper : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleSystemRenderer psRenderer;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        psRenderer = GetComponent<ParticleSystemRenderer>();

        if (psRenderer != null)
        {
            psRenderer.sortMode = ParticleSystemSortMode.Distance;
            psRenderer.alignment = ParticleSystemRenderSpace.View;

            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
        }

        Debug.Log("UIParticleHelper configurado correctamente");
    }

    void Update()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            transform.localScale = Vector3.one;
        }
    }
}