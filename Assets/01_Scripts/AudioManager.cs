using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    public AudioMixerGroup musicMixer;
    public AudioMixerGroup sfxMixer;

    private float musicVolume = 0.75f;
    private float sfxVolume = 0.75f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAudioSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        // Convierte de 0-1 a decibelios (-80 a 0)
        float db = musicVolume > 0 ? 20f * Mathf.Log10(musicVolume) : -80f;

        if (musicMixer != null)
        {
            musicMixer.audioMixer.SetFloat("MusicVolume", db);
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        // Convierte de 0-1 a decibelios (-80 a 0)
        float db = sfxVolume > 0 ? 20f * Mathf.Log10(sfxVolume) : -80f;

        if (sfxMixer != null)
        {
            sfxMixer.audioMixer.SetFloat("SFXVolume", db);
        }
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    public void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    public void LoadAudioSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }
}