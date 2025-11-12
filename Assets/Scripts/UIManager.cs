using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    [Header("Referensi Notifikasi EVENT (Timed)")]
    public GameObject notificationPanel; // Panel pop-up (Ikan lolos!)
    public TextMeshProUGUI notificationText;
    public float notificationDuration = 2.5f;
    private Coroutine notificationCoroutine;

    [Header("Notifikasi KONTEKS (Persistent Asset)")]
    public Image persistentImagePrompt;
    public Sprite promptAsset;

    [Header("Resting Asset")]
    public Image restingImage;
    public Sprite restingAsset;

    // [Header("Referensi Notifikasi KONTEKS (Persistent)")]
    // public GameObject persistentPanel; // Panel "Tekan E"
    // public TextMeshProUGUI persistentText;

    [Header("Referensi Chat Bubble")]
    public PlayerChatBubble playerChatBubble;

    void Start()
    {
        // Sembunyikan kedua panel di awal
        if (notificationPanel != null) notificationPanel.SetActive(false);
        if (persistentImagePrompt != null) persistentImagePrompt.gameObject.SetActive(false);
        if (restingImage != null) restingImage.gameObject.SetActive(false);

        // Debug: Cek apakah semua field sudah di-assign
        Debug.Log($"UIManager Check - Resting Image: {(restingImage != null ? "OK" : "NOT ASSIGNED")}");
        Debug.Log($"UIManager Check - Resting Asset: {(restingAsset != null ? "OK" : "NOT ASSIGNED")}");

        // Load volume settings dari PlayerPrefs ke AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.LoadVolumeSettings();
        }

        SaveSystem.instance.LoadGameData();
    }

    // --- Ini adalah fungsi LAMA, sekarang khusus untuk EVENT ---
    // (Dipanggil untuk "Ikan Lolos", "Energi Habis", dll.)
    public void ShowNotification(string message)
    {
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogWarning("UIManager: notificationPanel atau notificationText belum di-assign di Inspector!");
            return;
        }
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }
        notificationCoroutine = StartCoroutine(NotificationCoroutine(message));
    }

    private IEnumerator NotificationCoroutine(string message)
    {
        notificationText.text = message;
        notificationPanel.SetActive(true);
        yield return new WaitForSeconds(notificationDuration);
        notificationPanel.SetActive(false);
        notificationCoroutine = null;
    }

    public void ShowPersistentNotification(Sprite sprite)
    {
        if (persistentImagePrompt != null && sprite != null)
        {
            persistentImagePrompt.sprite = sprite;
            persistentImagePrompt.gameObject.SetActive(true);
        }
    }

    public void HidePersistentNotification()
    {
        if (persistentImagePrompt != null)
        {
            persistentImagePrompt.gameObject.SetActive(false);
        }
    }

    public void ShowRestingImage()
    {
        if (restingImage == null)
        {
            Debug.LogWarning("UIManager: restingImage belum di-assign di Inspector!");
            return;
        }
        
        if (restingAsset == null)
        {
            Debug.LogWarning("UIManager: restingAsset belum di-assign di Inspector!");
            return;
        }
        
        restingImage.sprite = restingAsset;
        StartCoroutine(FadeInRestingImage());
    }

    public void HideRestingImage()
    {
        if (restingImage != null)
        {
            StartCoroutine(FadeOutRestingImage());
        }
    }

    // --- Coroutine untuk Fade In Resting Image ---
    private IEnumerator FadeInRestingImage()
    {
        restingImage.gameObject.SetActive(true);
        Color color = restingImage.color;
        color.a = 0f;
        restingImage.color = color;

        float fadeDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            restingImage.color = color;
            yield return null;
        }

        color.a = 1f;
        restingImage.color = color;
        Debug.Log("Resting Image Fade In selesai!");
    }

    // --- Coroutine untuk Fade Out Resting Image ---
    private IEnumerator FadeOutRestingImage()
    {
        Color color = restingImage.color;
        color.a = 1f;
        restingImage.color = color;

        float fadeDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            restingImage.color = color;
            yield return null;
        }

        color.a = 0f;
        restingImage.color = color;
        restingImage.gameObject.SetActive(false);
        Debug.Log("Resting Image Fade Out selesai!");
    }

    public void ShowPlayerBubble(string message)
    {
        if (playerChatBubble != null)
        {
            playerChatBubble.Show(message);
        }
        else
        {
            ShowNotification(message);
        }
    }

}