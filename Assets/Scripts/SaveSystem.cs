using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem instance;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void SaveGameData()
    {
        Debug.Log("--- MENYIMPAN DATA... ---");
        
        // 1. DATA UMUM
        int currentDay = TimeSystem.instance.GetCurrentDay();
        int money = InventorySystem.instance.money;
        int baitCount = InventorySystem.instance.baitCount;
        
        // Ikan
        int smallFish = InventorySystem.instance.smallFishCount;
        int mediumFish = InventorySystem.instance.mediumFishCount;
        int bigFish = InventorySystem.instance.bigFishCount;
        
        // Perahu
        bool hasSmallBoat = InventorySystem.instance.hasSmallBoat;
        bool ownsBigBoat = InventorySystem.instance.ownsBigBoat;

        // 2. DATA MISI
        int missionSold = 0;
        bool missionComplete = false;
        if (DailyMissionManager.instance != null) {
            missionSold = DailyMissionManager.instance.currentSoldToday;
            missionComplete = DailyMissionManager.instance.isMissionComplete;
        }

        // 3. DATA KOLEKSI
        string unlockedFishData = "";
        if (CollectionManager.instance != null) {
            for (int i = 0; i < CollectionManager.instance.fishDatabase.Length; i++) {
                unlockedFishData += CollectionManager.instance.fishDatabase[i].isUnlocked ? "1" : "0";
            }
        }
        
        // --- SIMPAN KE PLAYERPREFS ---
        PlayerPrefs.SetInt("Save_CurrentDay", currentDay);
        PlayerPrefs.SetInt("Save_Money", money);
        PlayerPrefs.SetInt("Save_BaitCount", baitCount);
        
        PlayerPrefs.SetInt("Save_SmallFish", smallFish);
        PlayerPrefs.SetInt("Save_MediumFish", mediumFish);
        PlayerPrefs.SetInt("Save_BigFish", bigFish);
        
        PlayerPrefs.SetInt("Save_HasSmallBoat", hasSmallBoat ? 1 : 0);
        PlayerPrefs.SetInt("Save_OwnsBigBoat", ownsBigBoat ? 1 : 0);

        PlayerPrefs.SetInt("Save_MissionSold", missionSold);
        PlayerPrefs.SetInt("Save_MissionComplete", missionComplete ? 1 : 0);
        
        PlayerPrefs.SetString("Save_Collection", unlockedFishData);

        PlayerPrefs.Save();
        Debug.Log("--- DATA TERSIMPAN SUKSES! ---");
    }

    public void LoadGameData()
    {
        if (PlayerPrefs.HasKey("Save_CurrentDay"))
        {
            Debug.Log("--- MEMUAT DATA GAME... ---");

            // 1. LOAD VARIABEL
            int currentDay = PlayerPrefs.GetInt("Save_CurrentDay");
            int money = PlayerPrefs.GetInt("Save_Money");
            int baitCount = PlayerPrefs.GetInt("Save_BaitCount");
            
            int smallFish = PlayerPrefs.GetInt("Save_SmallFish", 0);
            int mediumFish = PlayerPrefs.GetInt("Save_MediumFish", 0);
            int bigFish = PlayerPrefs.GetInt("Save_BigFish", 0);
            
            bool hasSmallBoat = PlayerPrefs.GetInt("Save_HasSmallBoat") == 1;
            bool ownsBigBoat = PlayerPrefs.GetInt("Save_OwnsBigBoat") == 1;

            // 2. DISTRIBUSI DATA
            if (TimeSystem.instance != null) TimeSystem.instance.LoadDay(currentDay);
            
            if (InventorySystem.instance != null) 
                InventorySystem.instance.LoadInventory(money, smallFish, mediumFish, bigFish, baitCount, hasSmallBoat, ownsBigBoat);
            
            // 3. LOAD MISI (KUNCI PERBAIKANNYA DI SINI)
            if (DailyMissionManager.instance != null)
            {
                int missionSold = PlayerPrefs.GetInt("Save_MissionSold", 0);
                bool missionComplete = PlayerPrefs.GetInt("Save_MissionComplete", 0) == 1;

                // Fungsi ini akan mereset target sesuai hari, LALU menimpa dengan progress yang tersimpan
                DailyMissionManager.instance.LoadMissionProgress(currentDay, missionSold, missionComplete);
            }

            // 4. LOAD KOLEKSI
            if (PlayerPrefs.HasKey("Save_Collection") && CollectionManager.instance != null) {
                string savedData = PlayerPrefs.GetString("Save_Collection");
                for (int i = 0; i < savedData.Length; i++) {
                    if (i < CollectionManager.instance.fishDatabase.Length) {
                        CollectionManager.instance.fishDatabase[i].isUnlocked = (savedData[i] == '1');
                    }
                }
                CollectionManager.instance.UpdateCollectionUI(); 
            }
        }
        else
        {
            Debug.Log("Tidak ada save data. New Game.");
        }
    }
    
    // Fungsi bantuan lain (Hapus, Cek) biarkan saja...
    public static bool HasSaveData() { return PlayerPrefs.HasKey("Save_CurrentDay"); }
    public static void DeleteAllSaveData() { PlayerPrefs.DeleteAll(); PlayerPrefs.Save(); }
}