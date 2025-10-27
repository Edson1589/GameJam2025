using UnityEngine;
using System.Collections;
using TMPro; // Necesario si usas Text Mesh Pro

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI TextCredits;

    [TextArea(3, 10)]
    public string fullText = "Game made by:\r\nHector Manuel Arce León\r\nEdson Marcelo Cayo Ali\r\nDavid Andres Escalera Rocha\r\nEduardo Antezana Jau\r\nThanks for playing!";

    public float delay = 0.05f;

    void Start()
    {
        StartCoroutine(ShowText());
    }

    IEnumerator ShowText()
    {
        TextCredits.text = "";
        foreach (char character in fullText)
        {
            TextCredits.text += character;

            yield return new WaitForSeconds(delay);
        }

    }
}