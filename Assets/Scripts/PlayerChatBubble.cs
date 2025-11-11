using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerChatBubble : MonoBehaviour
{
    [Header("Referensi")]
    public TextMeshProUGUI textElement;
    public GameObject backgroundPanel;
    public float defaultDuration = 2.0f; // Durasi default gelembung

    private Coroutine showCoroutine;

    void Start()
    {
        // Sembunyikan gelembung di awal
        Hide();
    }

    /**
     * Fungsi utama untuk menampilkan pesan
     */
    public void Show(string message)
    {
        // Jika sedang menampilkan, hentikan dulu
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }
        // Mulai coroutine baru
        showCoroutine = StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        // Tampilkan
        textElement.text = message;
        backgroundPanel.SetActive(true);
        textElement.gameObject.SetActive(true);

        // Tunggu
        yield return new WaitForSeconds(defaultDuration);

        // Sembunyikan
        Hide();
        showCoroutine = null;
    }

    public void Hide()
    {
        backgroundPanel.SetActive(false);
        textElement.gameObject.SetActive(false);
    }
}