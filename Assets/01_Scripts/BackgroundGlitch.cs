using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundGlitch : MonoBehaviour
{
    [Header("Configuración de Glitch")]
    [SerializeField] private float glitchInterval = 3f; // Cada cuántos segundos hace glitch
    [SerializeField] private float glitchDuration = 0.1f; // Duración del glitch
    [SerializeField] private float glitchIntensity = 20f; // Intensidad del desplazamiento

    [Header("Parpadeo de Color (Opcional)")]
    [SerializeField] private bool enableColorFlicker = true;
    [SerializeField] private Color glitchColor = new Color(0f, 0.8f, 1f, 0.3f); // Azul claro

    private RectTransform rectTransform;
    private RawImage image;
    private Vector2 originalPosition;
    private Color originalColor;
    private float nextGlitchTime;
    private bool isGlitching;
    private float glitchTimer;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<RawImage>();

        originalPosition = rectTransform.anchoredPosition;
        if (image != null)
            originalColor = image.color;

        nextGlitchTime = Time.time + glitchInterval;
    }

    void Update()
    {
        if (!isGlitching && Time.time >= nextGlitchTime)
        {
            StartGlitch();
        }

        if (isGlitching)
        {
            glitchTimer -= Time.deltaTime;

            // Desplazamiento aleatorio
            float offsetX = Random.Range(-glitchIntensity, glitchIntensity);
            float offsetY = Random.Range(-glitchIntensity * 0.5f, glitchIntensity * 0.5f);
            rectTransform.anchoredPosition = originalPosition + new Vector2(offsetX, offsetY);

            // Cambio de color aleatorio
            if (enableColorFlicker && image != null && Random.value > 0.7f)
            {
                image.color = Color.Lerp(originalColor, glitchColor, Random.value);
            }

            if (glitchTimer <= 0)
            {
                EndGlitch();
            }
        }
    }

    private void StartGlitch()
    {
        isGlitching = true;
        glitchTimer = glitchDuration;
    }

    private void EndGlitch()
    {
        isGlitching = false;
        rectTransform.anchoredPosition = originalPosition;
        if (image != null)
            image.color = originalColor;

        nextGlitchTime = Time.time + glitchInterval + Random.Range(-1f, 2f);
    }
}
