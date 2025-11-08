using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Components;
using System.Collections;

public class ForceLocaleUpdate : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(WaitForLocalizationReady());
    }

    IEnumerator WaitForLocalizationReady()
    {
        yield return LocalizationSettings.InitializationOperation;

        yield return new WaitForSeconds(0.1f);

        LocalizeStringEvent[] localizers = FindObjectsOfType<LocalizeStringEvent>();

        foreach (LocalizeStringEvent localizer in localizers)
        {
            localizer.RefreshString();
        }

        Debug.Log("Textos visuales forzados a actualizarse. Deberían aparecer ahora.");
    }
}