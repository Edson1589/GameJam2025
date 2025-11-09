using UnityEngine;
using TMPro;

public class PlayerAmmoSystem : MonoBehaviour
{
    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo = 10;
    [SerializeField] private int currentAmmo = 0;
    [SerializeField] private int startingAmmo = 0;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private string ammoTextFormat = "Munición: {0}/{1}";

    [Header("Audio")]
    [SerializeField] private AudioClip noAmmoSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 0.7f;
    private AudioSource audioSource;

    public static PlayerAmmoSystem Instance { get; private set; }

    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo;
    public bool HasAmmo => currentAmmo > 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentAmmo = startingAmmo;
        currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);

        if (noAmmoSound != null || reloadSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound para UI
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    /// <summary>
    /// Agrega munición al inventario
    /// </summary>
    public void AddAmmo(int amount)
    {
        if (amount <= 0) return;

        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        UpdateUI();

        if (reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reloadSound, soundVolume);
        }
    }

    /// <summary>
    /// Consume munición. Retorna true si se pudo consumir, false si no hay munición
    /// </summary>
    public bool ConsumeAmmo(int amount = 1)
    {
        if (currentAmmo < amount)
        {
            // No hay suficiente munición
            if (noAmmoSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(noAmmoSound, soundVolume);
            }
            return false;
        }

        currentAmmo -= amount;
        currentAmmo = Mathf.Max(0, currentAmmo);
        UpdateUI();
        return true;
    }

    /// <summary>
    /// Verifica si hay munición disponible sin consumirla
    /// </summary>
    public bool CanShoot(int amount = 1)
    {
        return currentAmmo >= amount;
    }

    /// <summary>
    /// Establece la munición actual (útil para cheats o reset)
    /// </summary>
    public void SetAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(amount, 0, maxAmmo);
        UpdateUI();
    }

    /// <summary>
    /// Recarga munición al máximo
    /// </summary>
    public void ReloadToMax()
    {
        currentAmmo = maxAmmo;
        UpdateUI();

        if (reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reloadSound, soundVolume);
        }
    }

    private void UpdateUI()
    {
        if (ammoText != null)
        {
            ammoText.text = string.Format(ammoTextFormat, currentAmmo, maxAmmo);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

