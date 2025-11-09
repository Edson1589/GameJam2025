using UnityEngine;
using UnityEngine.Localization.Settings;

public class LevelInitializer : MonoBehaviour
{
    void Start()
    {
        if (LanguageSwitcher.Instance != null)
        {
            string savedLang = PlayerPrefs.GetString("SelectedLanguage", "");

            if (!string.IsNullOrEmpty(savedLang))
            {
                var targetLocale = LocalizationSettings.AvailableLocales.GetLocale(savedLang);
                if (targetLocale != null)
                {
                    LocalizationSettings.SelectedLocale = targetLocale;
                }
            }
        }
    }
}