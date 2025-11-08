using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class LanguageSwitcher : MonoBehaviour
{
    public static LanguageSwitcher Instance;
    private bool isReady = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(InitializeLocalization());
    }

    IEnumerator InitializeLocalization()
    {
        yield return LocalizationSettings.InitializationOperation;
        isReady = true;
    }

    public void SetLocale(string localeCode)
    {
        if (!isReady) return;

        List<Locale> availableLocales = LocalizationSettings.AvailableLocales.Locales;

        foreach (var locale in availableLocales)
        {
            if (locale.Identifier.Code.StartsWith(localeCode, System.StringComparison.OrdinalIgnoreCase))
            {
                if (LocalizationSettings.SelectedLocale != locale)
                {
                    LocalizationSettings.SelectedLocale = locale;
                    Debug.Log("Idioma cambiado a: " + locale.Identifier.Code);
                }

                // Forzamos la actualización de la selección del idioma
                LocalizationSettings.SelectedLocale = locale;

                // Cargar la escena de menú principal
                SceneManager.LoadScene("MainMenu");
                return;
            }
        }
        Debug.LogWarning("No se encontró el Locale con el código: " + localeCode);
    }
}