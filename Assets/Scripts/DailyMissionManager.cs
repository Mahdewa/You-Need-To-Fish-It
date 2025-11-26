using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DailyMissionManager : MonoBehaviour
{
    public static DailyMissionManager instance;

    void Awake()
    {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    [Header("Konfigurasi Misi")]
    // Array: Element 0 = Target Hari ke-1, Element 1 = Target Hari ke-2, dst.
    // PASTIKAN INI DIISI ANGKA > 0 DI INSPECTOR
    public int[] dailyFishQuotas; 
    
    [Header("Status Misi Saat Ini")]
    public int targetFishToday = 5; // Default 5 biar aman
    public int currentSoldToday = 0;
    public bool isMissionComplete = false;

    [Header("UI Referensi")]
    public TextMeshProUGUI missionText; 

    void Start()
    {
        // Hanya jalankan start baru jika belum ada data yang dimuat
        // (Biasanya LoadGameData akan menimpa ini nanti jika ada save file)
        if (TimeSystem.instance != null) {
            StartNewDayMission(TimeSystem.instance.GetCurrentDay());
        }
    }

    // Dipanggil saat hari berganti (setelah tidur) ATAU saat Load Game
    public void StartNewDayMission(int dayIndex)
    {
        // 1. Reset progress (Default)
        currentSoldToday = 0;
        isMissionComplete = false;

        // 2. Hitung Target berdasarkan Hari
        int arrayIndex = dayIndex - 1; // Hari 1 = Index 0

        // Cek apakah array ada isinya
        if (dailyFishQuotas != null && dailyFishQuotas.Length > 0)
        {
            if (arrayIndex >= 0 && arrayIndex < dailyFishQuotas.Length)
            {
                targetFishToday = dailyFishQuotas[arrayIndex];
            }
            else
            {
                // Jika hari melebihi data array, pakai data terakhir + (beda hari * 2)
                int lastTarget = dailyFishQuotas[dailyFishQuotas.Length - 1];
                int extraDays = dayIndex - dailyFishQuotas.Length;
                targetFishToday = lastTarget + (extraDays * 2);
            }
        }
        else
        {
            // Fallback jika lupa isi Inspector
            targetFishToday = 5 + (dayIndex * 2); 
            Debug.LogWarning("DailyFishQuotas kosong! Menggunakan target default.");
        }

        // SAFETY CHECK: Jangan sampai target 0
        if (targetFishToday <= 0) targetFishToday = 5;

        UpdateMissionUI();
    }

    // --- FUNGSI LOAD (DIPERBAIKI) ---
    public void LoadMissionProgress(int loadedDay, int soldAmount, bool isCompleted)
    {
        // 1. Hitung ulang target untuk hari yang di-load
        StartNewDayMission(loadedDay);

        // 2. Timpa data progress dengan data save
        currentSoldToday = soldAmount;
        isMissionComplete = isCompleted;

        // 3. Cek logika visual
        if (isMissionComplete)
        {
            // Jika status SAVE bilang selesai, paksa visual selesai
            // (Meski mungkin soldAmount < target karena perubahan array, tetap hormati save data)
            currentSoldToday = targetFishToday; 
            if (missionText != null) missionText.text = "<s>TARGET TERCAPAI</s>";
        }
        else
        {
            // Jika BELUM selesai, pastikan visual update normal
            // SAFETY: Jika karena bug target jadi 0, paksa update lagi
            if (targetFishToday <= 0) targetFishToday = 5;
            
            UpdateMissionUI();
        }

        Debug.Log($"Misi Dimuat: Hari {loadedDay} | Target {targetFishToday} | Terjual {currentSoldToday} | Status {isMissionComplete}");
    }

    // Dipanggil saat Player menjual ikan
    public void AddProgress(int amountSold)
    {
        if (isMissionComplete) return; 

        currentSoldToday += amountSold;

        // Cek apakah target tercapai
        if (currentSoldToday >= targetFishToday)
        {
            CompleteMission();
        }

        UpdateMissionUI();
    }

    private void CompleteMission()
    {
        isMissionComplete = true;
        currentSoldToday = targetFishToday; // Cap di max
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("mission");
        if (missionText != null) missionText.text = "<s>TARGET TERCAPAI</s>";
    }

    // Dipanggil SEBELUM Player tidur
    public bool CanSleep()
    {
        if (isMissionComplete)
        {
            return true; 
        }
        else
        {
            TriggerGameOver();
            return false;
        }
    }

    private void TriggerGameOver()
    {
        Debug.Log("GAME OVER: Target harian tidak tercapai!");
        SceneManager.LoadScene("MainMenu");
    }

    private void UpdateMissionUI()
    {
        if (missionText != null)
        {
            if (isMissionComplete)
            {
                missionText.text = "<s>TARGET TERCAPAI</s>";
            }
            else
            {
                missionText.text = $"JUAL {currentSoldToday}/{targetFishToday} IKAN";
            }
        }
    }
}