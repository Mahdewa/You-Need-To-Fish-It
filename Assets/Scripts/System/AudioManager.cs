using UnityEngine;
using System;

// Kelas kecil untuk menyimpan data suara agar mudah diatur di Inspector
[System.Serializable]
public class Sound
{
    public string name; // Nama untuk memanggil suara (misal: "Jump", "MainMenuTheme")
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    // Singleton pattern agar skrip ini bisa diakses dari mana saja
    public static AudioManager Instance;

    [Header("Daftar Audio")]
    [SerializeField] private Sound[] bgmSounds; // Array untuk Background Music
    [SerializeField] private Sound[] sfxSounds; // Array untuk Sound Effects

    [Header("Sumber Audio")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Nilai Default Volume (0-1)")]
    [SerializeField] private float defaultBGMVolume = 0.5f;
    [SerializeField] private float defaultSFXVolume = 0.5f;

    // Player Prefs Keys
    private const string BGM_VOLUME_KEY = "BGM_Volume";
    private const string SFX_VOLUME_KEY = "SFX_Volume";

    private void Awake()
    {
        // Singleton Pattern Implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Perintah kunci agar tidak hancur saat pindah scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Load volume settings dari PlayerPrefs
        LoadVolumeSettings();
    }

    // --- LOAD VOLUME SETTINGS ---
    public void LoadVolumeSettings()
    {
        // Load BGM Volume
        float bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, defaultBGMVolume);
        bgmSource.volume = bgmVolume;

        // Load SFX Volume
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSFXVolume);
        sfxSource.volume = sfxVolume;

        Debug.Log($"Volume Settings Loaded - BGM: {bgmVolume}, SFX: {sfxVolume}");
    }

    // Memainkan BGM berdasarkan nama yang diberikan
    // Jika BGM yang sama sudah playing, tidak akan di-reset/diulang
    public void PlayBGM(string soundName)
    {
        Sound s = Array.Find(bgmSounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("BGM dengan nama: " + soundName + " tidak ditemukan!");
            return;
        }

        // CEK: Jika musik yang sama sudah playing, jangan ulang
        if (bgmSource.clip == s.clip && bgmSource.isPlaying)
        {
            Debug.Log($"BGM '{soundName}' sudah playing, tidak di-reset");
            return;
        }

        // Jika beda musik, baru ganti dan play
        bgmSource.clip = s.clip;
        bgmSource.Play();
        Debug.Log($"Playing BGM: {soundName}");
    }

    // Memainkan SFX berdasarkan nama yang diberikan
    public void PlaySFX(string soundName)
    {
        Sound s = Array.Find(sfxSounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("SFX dengan nama: " + soundName + " tidak ditemukan!");
            return;
        }
        sfxSource.PlayOneShot(s.clip); // PlayOneShot agar tidak menimpa SFX lain
    }

    // Method untuk mengubah volume BGM (dipanggil oleh slider) dan menyimpannya
    public void ChangeBGMVolume(float volume)
    {
        bgmSource.volume = volume;
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, volume);
        PlayerPrefs.Save();
        Debug.Log($"BGM Volume Changed to: {volume}");
    }
    
    // Method untuk mengubah volume SFX (dipanggil oleh slider) dan menyimpannya
    public void ChangeSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
        PlayerPrefs.Save();
        Debug.Log($"SFX Volume Changed to: {volume}");
    }

    // Method untuk mendapatkan volume BGM (berguna untuk UI slider)
    public float GetBGMVolume()
    {
        return PlayerPrefs.GetFloat(BGM_VOLUME_KEY, defaultBGMVolume);
    }

    // Method untuk mendapatkan volume SFX (berguna untuk UI slider)
    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSFXVolume);
    }

    // Method untuk pause BGM (ketika pergi ke menu/scene lain, musik tetap tersimpan)
    public void PauseBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Pause();
            Debug.Log("BGM Paused");
        }
    }

    // Method untuk resume BGM (melanjutkan musik yang sudah di-pause)
    public void ResumeBGM()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.Play();
            Debug.Log("BGM Resumed");
        }
    }

    // Method untuk stop BGM
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
            Debug.Log("BGM Stopped");
        }
    }

    // Method untuk cek apakah BGM sedang playing
    public bool IsBGMPlaying()
    {
        return bgmSource != null && bgmSource.isPlaying;
    }

    // Method untuk stop SFX (menghentikan SFX yang sedang playing)
    public void StopSFX()
    {
        if (sfxSource != null && sfxSource.isPlaying)
        {
            sfxSource.Stop();
            Debug.Log("SFX Stopped");
        }
    }
}