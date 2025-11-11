using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CutscenePanel
{
    public Sprite panelImage;
    [TextArea(3, 10)]
    public string narrationText;
}

public class CutsceneManager : MonoBehaviour
{
    [Header("Data Cutscene")]
    [SerializeField] private List<CutscenePanel> panels;
    [Tooltip("HANYA UNTUK PROLOG: Nama scene yang akan dimuat setelah cutscene selesai.")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Referensi Komponen UI")]
    [SerializeField] private Image panelImageUI;
    [SerializeField] private TextMeshProUGUI narrationTextUI;
    [SerializeField] private CanvasGroup cutsceneCanvasGroup;

    [Header("Pengaturan Khusus Epilog")]
    [Tooltip("Centang ini jika cutscene ini adalah Epilog.")]
    [SerializeField] private bool isEpilogue = false;
    [Tooltip("Tombol yang akan muncul di akhir Epilog.")]
    [SerializeField] private GameObject endButton;

    private int currentPanelIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        if (isEpilogue && endButton != null)
        {
            // Pastikan tombol dan Canvas Group-nya nonaktif & transparan di awal
            endButton.SetActive(false);
            CanvasGroup buttonCG = endButton.GetComponent<CanvasGroup>();
            if (buttonCG != null) buttonCG.alpha = 0;
        }

        if(cutsceneCanvasGroup != null) cutsceneCanvasGroup.alpha = 0;
        StartCoroutine(TransitionToPanel(0));
    }

    void Update()
    {
        if (Input.anyKeyDown && !isTransitioning)
        {
            AdvanceToNextPanel();
        }
    }

    private void AdvanceToNextPanel()
    {
        currentPanelIndex++;

        if (currentPanelIndex >= panels.Count)
        {
            EndCutscene();
        }
        else
        {
            StartCoroutine(TransitionToPanel(currentPanelIndex));
        }
    }

    // --- PERUBAHAN UTAMA ADA DI SINI ---
    private IEnumerator TransitionToPanel(int index)
    {
        isTransitioning = true;
        yield return StartCoroutine(Fade(0f)); // Fade Out

        if (index < panels.Count)
        {
            panelImageUI.sprite = panels[index].panelImage;
            narrationTextUI.text = panels[index].narrationText;
        }

        // Jika ini adalah panel terakhir dari epilog, mulai proses fade in tombol
        if (isEpilogue && index == panels.Count - 1)
        {
            StartCoroutine(FadeInButton());
        }

        yield return StartCoroutine(Fade(1f)); // Fade In
        isTransitioning = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float timer = 0f;
        float startAlpha = cutsceneCanvasGroup.alpha;

        while (timer < fadeDuration)
        {
            cutsceneCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        cutsceneCanvasGroup.alpha = targetAlpha;
    }

    // --- EndCutscene kini lebih sederhana untuk epilog ---
    private void EndCutscene()
    {
        isTransitioning = true; // Hentikan input lebih lanjut

        // Untuk Epilog, tidak terjadi apa-apa. Cutscene "selesai" saat panel terakhir muncul.
        // Pemain harus mengklik tombol yang sudah muncul.

        // Jika ini Prolog, muat scene berikutnya
        if (!isEpilogue && !string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    // Coroutine ini sekarang khusus untuk fade in tombol
    private IEnumerator FadeInButton()
    {
        if (endButton != null)
        {
            endButton.SetActive(true);
            CanvasGroup buttonCG = endButton.GetComponent<CanvasGroup>();
            if (buttonCG != null)
            {
                float timer = 0f;
                // Fade in tombol secara perlahan
                while (timer < fadeDuration)
                {
                    buttonCG.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
                    timer += Time.deltaTime;
                    yield return null;
                }
                buttonCG.alpha = 1f;
            }
        }
    }
}