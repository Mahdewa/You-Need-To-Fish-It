using UnityEngine;

public class ButtonSfx : MonoBehaviour
{
    public void PlayClickSfx()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("button");
        }
        else
        {
            Debug.LogError("AudioManager tidak ditemukan! Pastikan scene Main Menu dijalankan pertama kali.");
        }
    }
}