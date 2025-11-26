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
    public int[] dailyFishQuotas; 
    
    [Header("Status Misi Saat Ini")]
    public int targetFishToday = 5; 
    public int currentSoldToday = 0;
    public bool isMissionComplete = false;
    
    // VARIABEL BARU: Mode Bebas
    public bool isFreeMode = false; 

    [Header("UI Referensi")]
    public TextMeshProUGUI missionText; 

    void Start()
    {
        if (TimeSystem.instance != null) {
            StartNewDayMission(TimeSystem.instance.GetCurrentDay());
        }
    }

    // Dipanggil saat hari berganti ATAU saat Load Game
    public void StartNewDayMission(int dayIndex)
    {
        // 1. Reset progress awal
        currentSoldToday = 0;
        isMissionComplete = false;
        isFreeMode = false; // Defaultnya matikan dulu

        int arrayIndex = dayIndex - 1; // Hari 1 = Index 0

        // 2. Cek apakah Array Misi Valid
        if (dailyFishQuotas != null && dailyFishQuotas.Length > 0)
        {
            // Jika hari ini MASIH ADA di daftar misi
            if (arrayIndex < dailyFishQuotas.Length)
            {
                targetFishToday = dailyFishQuotas[arrayIndex];
                isFreeMode = false;
            }
            // Jika hari ini SUDAH MELEWATI daftar misi (Habis)
            else
            {
                isFreeMode = true; // Aktifkan Mode Bebas!
            }
        }
        else
        {
            // Fallback jika lupa isi Inspector (anggap masih ada misi default)
            targetFishToday = 5; 
            isFreeMode = false;
        }

        // SAFETY: Jangan sampai target 0 jika bukan free mode
        if (!isFreeMode && targetFishToday <= 0) targetFishToday = 5;

        UpdateMissionUI();
    }

    public void LoadMissionProgress(int loadedDay, int soldAmount, bool isCompleted)
    {
        // 1. Hitung ulang target / Cek Free Mode berdasarkan hari
        StartNewDayMission(loadedDay);

        // 2. Timpa data progress
        currentSoldToday = soldAmount;
        isMissionComplete = isCompleted;

        // 3. Update Visual
        if (isFreeMode)
        {
             UpdateMissionUI(); // Akan menampilkan "Enjoy your Days!"
        }
        else if (isMissionComplete)
        {
            currentSoldToday = targetFishToday; 
            if (missionText != null) missionText.text = "<s>TARGET TERCAPAI</s>";
        }
        else
        {
            UpdateMissionUI();
        }

        Debug.Log($"Misi Dimuat: Hari {loadedDay} | FreeMode: {isFreeMode}");
    }

    public void AddProgress(int amountSold)
    {
        // Jika Free Mode, tidak perlu hitung progress
        if (isFreeMode) return;
        if (isMissionComplete) return; 

        currentSoldToday += amountSold;

        if (currentSoldToday >= targetFishToday)
        {
            CompleteMission();
        }

        UpdateMissionUI();
    }

    private void CompleteMission()
    {
        isMissionComplete = true;
        currentSoldToday = targetFishToday; 
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("mission"); // Pastikan nama SFX benar
        if (missionText != null) missionText.text = "<s>TARGET TERCAPAI</s>";
    }

    // Dipanggil SEBELUM Player tidur
    public bool CanSleep()
    {
        // PERUBAHAN PENTING DI SINI:
        // Jika Free Mode aktif, BOLEH tidur kapan saja (Return true)
        if (isFreeMode)
        {
            return true;
        }

        // Jika tidak Free Mode, cek apakah misi selesai
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
        SceneManager.LoadScene("MainMenu"); // Atau scene GameOver
    }

    private void UpdateMissionUI()
    {
        if (missionText != null)
        {
            // LOGIKA UI BARU
            if (isFreeMode)
            {
                missionText.text = "ENJOY YOUR DAYS! <3";
                // missionText.color = Color.cyan; // Opsional: Beri warna santai
            }
            else if (isMissionComplete)
            {
                missionText.text = "<s>TARGET TERCAPAI</s>";
                // missionText.color = Color.green;
            }
            else
            {
                missionText.text = $"JUAL {currentSoldToday}/{targetFishToday} IKAN";
                // missionText.color = Color.white;
            }
        }
    }
}