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

    // Memainkan BGM berdasarkan nama yang diberikan
    public void PlayBGM(string soundName)
    {
        Sound s = Array.Find(bgmSounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("BGM dengan nama: " + soundName + " tidak ditemukan!");
            return;
        }
        bgmSource.clip = s.clip;
        bgmSource.Play();
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

    // Method untuk mengubah volume BGM (dipanggil oleh slider)
    public void ChangeBGMVolume(float volume)
    {
        bgmSource.volume = volume;
    }
    
    // Method untuk mengubah volume SFX (dipanggil oleh slider)
    public void ChangeSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}