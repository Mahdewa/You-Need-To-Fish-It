using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{

    public string bgName;

    void Start()
    {
        // Cek apakah AudioManager sudah ada
        if (AudioManager.Instance != null)
        {
            // Perintahkan AudioManager untuk memainkan BGM dengan nama yang ditentukan
            AudioManager.Instance.PlayBGM(bgName);
        }
        else
        {
            Debug.LogError("AudioManager tidak ditemukan! Pastikan scene Main Menu dijalankan pertama kali.");
        }
    }
}