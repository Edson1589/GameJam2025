using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Components;

public class LanguageSwitcher : MonoBehaviour
{
    public static LanguageSwitcher Instance;
    private bool isReady = false;

    [Header("Settings")]
    [SerializeField] private bool useSystemLanguage = true;
    [SerializeField] private string fallbackLanguage = "en";
    [SerializeField] private string mainMenuSceneName = "00_Scenes/MainMenu";

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
        var initOp = LocalizationSettings.InitializationOperation;
        yield return initOp;

        if (!initOp.IsDone || initOp.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("Error al inicializar el sistema de localización");
            yield break;
        }

        isReady = true;

        string savedLanguage = PlayerPrefs.GetString("SelectedLanguage", "");

        if (!string.IsNullOrEmpty(savedLanguage))
        {
            Debug.Log($"Cargando idioma guardado: {savedLanguage}");
            SetLocaleWithoutSceneReload(savedLanguage);
        }
        else if (useSystemLanguage)
        {
            SetSystemLanguage();
        }

        yield return null;
    }

    private void SetSystemLanguage()
    {
        SystemLanguage systemLang = Application.systemLanguage;
        string localeCode = fallbackLanguage;

        switch (systemLang)
        {
            case SystemLanguage.Spanish:
                localeCode = "es-MX";
                break;
            case SystemLanguage.English:
                localeCode = "en-US";
                break;
            case SystemLanguage.Portuguese:
                localeCode = "pt-PT";
                break;
            case SystemLanguage.French:
                localeCode = "fr-FR";
                break;
            default:
                localeCode = fallbackLanguage;
                break;
        }

        Debug.Log($"Idioma del sistema: {systemLang} → Aplicando: {localeCode}");
        SetLocaleWithoutSceneReload(localeCode);
    }

    private void SetLocaleWithoutSceneReload(string localeCode)
    {
        if (!isReady)
        {
            Debug.LogWarning("Sistema de localización no está listo");
            return;
        }

        Locale targetLocale = GetLocaleByCode(localeCode);

        if (targetLocale != null)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
            Debug.Log($"Idioma aplicado: {targetLocale.Identifier.Code}");

            RefreshLocalizeComponents();

        }
        else
        {
            Debug.LogWarning($"No se encontró locale: {localeCode}, usando fallback: {fallbackLanguage}");
            targetLocale = GetLocaleByCode(fallbackLanguage);
            if (targetLocale != null)
            {
                LocalizationSettings.SelectedLocale = targetLocale;
                RefreshLocalizeComponents();
            }
        }
    }

    public void SetLocale(string localeCode)
    {
        if (!isReady)
        {
            Debug.LogWarning("Sistema de localización no está listo");
            return;
        }

        Locale targetLocale = GetLocaleByCode(localeCode);

        if (targetLocale != null)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
            Debug.Log($"Idioma cambiado manualmente a: {targetLocale.Identifier.Code}");

            PlayerPrefs.SetString("SelectedLanguage", localeCode);
            PlayerPrefs.Save();

            RefreshLocalizeComponents();

            StartCoroutine(LoadMainMenuAfterDelay(0.1f));
        }
        else
        {
            Debug.LogWarning($"No se encontró el locale: {localeCode}");
        }
    }

    private IEnumerator LoadMainMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private Locale GetLocaleByCode(string localeCode)
    {
        List<Locale> availableLocales = LocalizationSettings.AvailableLocales.Locales;

        foreach (var locale in availableLocales)
        {
            if (locale.Identifier.Code.Equals(localeCode, System.StringComparison.OrdinalIgnoreCase))
            {
                return locale;
            }
        }

        return null;
    }

    private void RefreshLocalizeComponents()
    {
        LocalizeStringEvent[] allLocalizeEvents =
            FindObjectsByType<LocalizeStringEvent>(FindObjectsSortMode.None);

        foreach (var localizeEvent in allLocalizeEvents)
        {
            localizeEvent.RefreshString();
        }
    }

    public string GetSavedLanguage()
    {
        return PlayerPrefs.GetString("SelectedLanguage", "");
    }
}