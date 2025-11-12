using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    private static FadeManager instance;
    private Image fadeImage;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Buat canvas untuk fade image jika belum ada
        CreateFadeCanvas();
    }

    private void CreateFadeCanvas()
    {
        // Cek apakah sudah ada FadeCanvas
        GameObject existingCanvas = GameObject.Find("FadeCanvas");
        if (existingCanvas != null)
        {
            fadeImage = existingCanvas.GetComponentInChildren<Image>();
            canvasGroup = existingCanvas.GetComponent<CanvasGroup>();
            return;
        }

        // Buat Canvas baru
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        
        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // Buat Image untuk fade
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform rectTransform = imageObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = Color.black;
        
        Debug.Log("FadeCanvas dibuat otomatis!");
    }

    // Helper method untuk memastikan FadeManager ada
    private static void EnsureFadeManager()
    {
        if (instance == null)
        {
            // Cek apakah sudah ada FadeManager di scene
            FadeManager existingManager = FindObjectOfType<FadeManager>();
            if (existingManager == null)
            {
                // Buat FadeManager baru
                GameObject faderObj = new GameObject("FadeManager");
                faderObj.AddComponent<FadeManager>();
                Debug.Log("FadeManager dibuat otomatis!");
            }
        }
    }

    public static IEnumerator FadeToBlack(float duration)
    {
        EnsureFadeManager();

        if (instance == null || instance.canvasGroup == null)
        {
            Debug.LogWarning("FadeManager tidak bisa diinisialisasi!");
            yield break;
        }

        Debug.Log("Fade To Black dimulai...");
        
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            instance.canvasGroup.alpha = Mathf.Clamp01(elapsedTime / duration);
            yield return null;
        }

        instance.canvasGroup.alpha = 1f;
        Debug.Log("Fade To Black selesai!");
    }

    public static IEnumerator FadeFromBlack(float duration)
    {
        EnsureFadeManager();

        if (instance == null || instance.canvasGroup == null)
        {
            Debug.LogWarning("FadeManager tidak bisa diinisialisasi!");
            yield break;
        }

        Debug.Log("Fade From Black dimulai...");
        
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            instance.canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / duration));
            yield return null;
        }

        instance.canvasGroup.alpha = 0f;
        Debug.Log("Fade From Black selesai!");
    }
}
