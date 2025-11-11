using UnityEngine;
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

    [Header("Referensi Notifikasi KONTEKS (Persistent)")]
    public GameObject persistentPanel; // Panel "Tekan E"
    public TextMeshProUGUI persistentText;

    [Header("Referensi Chat Bubble")]
    public PlayerChatBubble playerChatBubble;

    void Start()
    {
        // Sembunyikan kedua panel di awal
        if (notificationPanel != null) notificationPanel.SetActive(false);
        if (persistentPanel != null) persistentPanel.SetActive(false);

        SaveSystem.instance.LoadGameData();
    }

    // --- Ini adalah fungsi LAMA, sekarang khusus untuk EVENT ---
    // (Dipanggil untuk "Ikan Lolos", "Energi Habis", dll.)
    public void ShowNotification(string message)
    {
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

    public void ShowPersistentNotification(string message)
    {
        if (persistentPanel != null)
        {
            persistentText.text = message;
            persistentPanel.SetActive(true);
        }
    }

    public void HidePersistentNotification()
    {
        if (persistentPanel != null)
        {
            persistentPanel.SetActive(false);
        }
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