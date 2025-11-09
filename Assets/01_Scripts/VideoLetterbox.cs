using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// VideoLetterbox - Añade barras negras (letterbox/pillarbox) a los lados del video
/// para cubrir las áreas que muestran el skybox de Unity
/// </summary>
public class VideoLetterbox : MonoBehaviour
{
    [Header("Letterbox Settings")]
    [SerializeField] private bool createOnStart = true;
    [SerializeField] private Color barColor = Color.black;
    
    [Header("Bar Width (Optional - Auto if 0)")]
    [SerializeField] private float barWidth = 0f; // Si es 0, se calcula automáticamente
    
    private Canvas letterboxCanvas;
    private Image leftBar;
    private Image rightBar;

    void Start()
    {
        if (createOnStart)
        {
            CreateLetterbox();
        }
    }

    public void CreateLetterbox()
    {
        // Crear Canvas si no existe
        if (letterboxCanvas == null)
        {
            GameObject canvasObj = new GameObject("LetterboxCanvas");
            letterboxCanvas = canvasObj.AddComponent<Canvas>();
            letterboxCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            letterboxCanvas.sortingOrder = 1000; // Asegurar que esté por encima de todo

            // Añadir Canvas Scaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Añadir Graphic Raycaster (opcional, pero recomendado)
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Crear barra izquierda
        if (leftBar == null)
        {
            leftBar = CreateBar("LeftBar", letterboxCanvas.transform);
            SetupLeftBar(leftBar);
        }

        // Crear barra derecha
        if (rightBar == null)
        {
            rightBar = CreateBar("RightBar", letterboxCanvas.transform);
            SetupRightBar(rightBar);
        }
    }

    private Image CreateBar(string name, Transform parent)
    {
        GameObject barObj = new GameObject(name);
        barObj.transform.SetParent(parent, false);
        
        Image image = barObj.AddComponent<Image>();
        image.color = barColor;
        
        RectTransform rectTransform = barObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        
        return image;
    }

    private void SetupLeftBar(Image bar)
    {
        RectTransform rect = bar.GetComponent<RectTransform>();
        
        // Anclar a la izquierda y estirar verticalmente
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 0.5f);
        
        // Calcular ancho
        float width = barWidth > 0 ? barWidth : CalculateBarWidth();
        rect.sizeDelta = new Vector2(width, 0);
        
        rect.anchoredPosition = new Vector2(0, 0);
    }

    private void SetupRightBar(Image bar)
    {
        RectTransform rect = bar.GetComponent<RectTransform>();
        
        // Anclar a la derecha y estirar verticalmente
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 0.5f);
        
        // Calcular ancho
        float width = barWidth > 0 ? barWidth : CalculateBarWidth();
        rect.sizeDelta = new Vector2(width, 0);
        
        rect.anchoredPosition = new Vector2(0, 0);
    }

    private float CalculateBarWidth()
    {
        // Calcular el ancho necesario basado en la resolución de pantalla
        // Asumimos que el video es 16:9 y la pantalla puede ser más ancha
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float screenAspect = screenWidth / screenHeight;
        float videoAspect = 16f / 9f; // Asumiendo video 16:9
        
        if (screenAspect > videoAspect)
        {
            // La pantalla es más ancha que el video, necesitamos barras laterales
            float videoWidth = screenHeight * videoAspect;
            float barWidth = (screenWidth - videoWidth) / 2f;
            return barWidth;
        }
        
        // Si la pantalla no es más ancha, usar un ancho fijo por seguridad
        return 400f;
    }

    public void RemoveLetterbox()
    {
        if (letterboxCanvas != null)
        {
            Destroy(letterboxCanvas.gameObject);
            letterboxCanvas = null;
            leftBar = null;
            rightBar = null;
        }
    }

    public void SetBarColor(Color color)
    {
        barColor = color;
        if (leftBar != null) leftBar.color = color;
        if (rightBar != null) rightBar.color = color;
    }
}

