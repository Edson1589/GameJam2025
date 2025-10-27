using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroll : MonoBehaviour
{
    public float scrollSpeed = 0.02f;
    private RawImage rawImage;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        if (rawImage != null)
        {
            Rect uvRect = rawImage.uvRect;
            uvRect.x += scrollSpeed * Time.deltaTime;
            rawImage.uvRect = uvRect;
        }
    }
}