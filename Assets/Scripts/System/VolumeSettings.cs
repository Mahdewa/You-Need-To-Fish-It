using UnityEngine;
using UnityEngine.UI; // Diperlukan untuk mengakses komponen UI seperti Slider

public class VolumeSettings : MonoBehaviour
{
    [Header("Referensi Slider")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    // Kunci untuk menyimpan data di PlayerPrefs
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    void Start()
    {
        // Muat pengaturan volume yang tersimpan saat scene dibuka
        LoadVolumeSettings();

        // Tambahkan "pendengar" agar slider bisa langsung mengubah volume saat digeser
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    private void LoadVolumeSettings()
    {
        // Muat volume BGM, jika tidak ada data, gunakan nilai default 0.75
        float bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.50f);
        bgmSlider.value = bgmVolume;
        AudioManager.Instance.ChangeBGMVolume(bgmVolume);

        // Muat volume SFX, jika tidak ada data, gunakan nilai default 0.75
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.50f);
        sfxSlider.value = sfxVolume;
        AudioManager.Instance.ChangeSFXVolume(sfxVolume);
    }

    public void SetBGMVolume(float volume)
    {
        // Perbarui volume di AudioManager
        AudioManager.Instance.ChangeBGMVolume(volume);
        // Simpan pengaturan ke memori perangkat
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, volume);
    }

    public void SetSFXVolume(float volume)
    {
        // Perbarui volume di AudioManager
        AudioManager.Instance.ChangeSFXVolume(volume);
        // Simpan pengaturan ke memori perangkat
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
    }
}