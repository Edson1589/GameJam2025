using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class VerticalLightBeam : MonoBehaviour
{
    [Header("Aspecto")]
    [SerializeField] private Color bottomColor = new Color(0.2f, 0.9f, 1f, 1f);
    [SerializeField] private Color topColor = new Color(0.2f, 0.9f, 1f, 0f);
    [SerializeField, Range(0.1f, 8f)] private float verticalFalloff = 2.5f;
    [SerializeField, Range(0f, 10f)] private float horizontalEdgeFeather = 0.25f;

    [Header("Textura")]
    [SerializeField, Range(16, 512)] private int texWidth = 64;
    [SerializeField, Range(64, 2048)] private int texHeight = 512;

    [Header("Pulso (opcional)")]
    [SerializeField] private bool pulse = true;
    [SerializeField, Range(0f, 1f)] private float pulseAmplitude = 0.2f;
    [SerializeField] private float pulseSpeed = 2f;

    private Renderer rend;
    private Material matInstance;
    private Texture2D tex;
    private float baseAlphaMul = 1f;

    void OnEnable()
    {
        rend = GetComponent<Renderer>();
        if (rend)
        {
            matInstance = Application.isPlaying ? rend.material : rend.sharedMaterial;
            if (matInstance != null)
            {
                GenerateTexture();
                baseAlphaMul = 1f;
            }
        }
    }

    void OnDisable()
    {
        if (Application.isPlaying && tex != null) Destroy(tex);
    }

    void OnValidate() { if (isActiveAndEnabled) GenerateTexture(); }

    void Update()
    {
        if (!matInstance) return;
        if (!pulse) return;

        float mul = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
        SetMaterialAlphaMultiplier(mul);
    }

    void SetMaterialAlphaMultiplier(float mul)
    {
        if (matInstance.HasProperty("_BaseColor"))
        {
            var c = matInstance.GetColor("_BaseColor");
            c.a = baseAlphaMul * mul;
            matInstance.SetColor("_BaseColor", c);
        }
        else if (matInstance.HasProperty("_Color"))
        {
            var c = matInstance.color;
            c.a = baseAlphaMul * mul;
            matInstance.color = c;
        }
    }

    void GenerateTexture()
    {
        if (!matInstance) return;

        if (tex != null)
        {
            if (Application.isPlaying) Destroy(tex);
#if UNITY_EDITOR
            else DestroyImmediate(tex);
#endif
        }

        tex = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false, true);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < texHeight; y++)
        {
            float v = (float)y / (texHeight - 1);
            float alphaV = Mathf.Pow(1f - v, verticalFalloff);

            for (int x = 0; x < texWidth; x++)
            {
                float u = (float)x / (texWidth - 1);
                float edge = Mathf.Abs(u * 2f - 1f);
                float feather = Mathf.InverseLerp(1f, 1f - horizontalEdgeFeather, edge);
                feather = 1f - feather;

                Color col = Color.Lerp(bottomColor, topColor, v);
                col.a *= alphaV * Mathf.Clamp01(feather);
                tex.SetPixel(x, y, col);
            }
        }

        tex.Apply();

        if (matInstance.HasProperty("_MainTex"))
            matInstance.SetTexture("_MainTex", tex);

        if (matInstance.HasProperty("_Color"))
        {
            var c = matInstance.color; c.a = 1f; matInstance.color = c;
        }
        if (matInstance.HasProperty("_BaseColor"))
        {
            var c = matInstance.GetColor("_BaseColor"); c.a = 1f; matInstance.SetColor("_BaseColor", c);
        }
    }
}
