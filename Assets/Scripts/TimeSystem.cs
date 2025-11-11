using UnityEngine;
using TMPro;
using UnityEngine.Rendering.Universal;

public class TimeSystem : MonoBehaviour
{
    // Singleton Pattern
    public static TimeSystem instance;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    [Header("Langit")]
    public SpriteRenderer morningSkySprite;
    public SpriteRenderer duskSkySprite;

    [Header("Referensi UI")]
    public TextMeshProUGUI dayText;

    [Header("Pengaturan Waktu")]
    public float secondsPerFullDay = 120f; // Waktu (detik) dari pagi ke senja
    
    // Status Internal
    private int currentDay = 1;
    private float currentTimeNormalized = 0f; // 0.0 = Pagi, 1.0 = Senja

    void Start() {
        UpdateSkyAssets(currentTimeNormalized);
        UpdateDayUI();
    }

    void Update() {
        if (currentTimeNormalized < 1.0f)
        {
            currentTimeNormalized += Time.deltaTime / secondsPerFullDay;
            currentTimeNormalized = Mathf.Clamp01(currentTimeNormalized);
        }

        UpdateSkyAssets(currentTimeNormalized);
    }

    void UpdateSkyAssets(float time) {
        if (morningSkySprite == null || duskSkySprite == null) return;

        Color duskColor = duskSkySprite.color;

        duskColor.a = time;
        duskSkySprite.color = duskColor;

        Color morningColor = morningSkySprite.color;

        morningColor.a = 1.0f - time; 
        morningSkySprite.color = morningColor;
    }

    void UpdateDayUI() {
        if (dayText != null)
        {
            dayText.text = "DAY - " + currentDay;
        }
    }

    public void PassDay() {
        currentTimeNormalized = 0f;
        currentDay++;
        Debug.Log($"Selamat pagi! Ini adalah HARI KE-{currentDay}");

        UpdateSkyAssets(currentTimeNormalized);
        UpdateDayUI();

        SaveSystem.instance.SaveGameData();
    }

    public int GetCurrentDay() {
        return currentDay;
    }

    public void LoadDay(int loadedDay)
    {
        currentDay = loadedDay;
        
        // Update UI hari saat me-load
        UpdateDayUI();
    }
}