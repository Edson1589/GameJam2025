using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.Localization;

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI TextCredits;

    [Header("Texto localizado")]
    [SerializeField] private LocalizedString localizedFullText;

    [Header("Configuración del efecto")]
    public float delay = 0.05f;

    private Coroutine currentCoroutine; 

    void OnEnable()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(ShowText());
    }

    void OnDisable() 
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
    }

    IEnumerator ShowText()
    {
        TextCredits.text = "";

        if (localizedFullText == null)
        {
            Debug.LogWarning("No se asignó localizedFullText en TypewriterEffect.");
            yield break;
        }

        string fullText = localizedFullText.GetLocalizedString();

        foreach (char character in fullText)
        {
            TextCredits.text += character;
            yield return new WaitForSeconds(delay);
        }

        currentCoroutine = null; 
    }
}