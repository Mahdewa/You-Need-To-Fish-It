using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    // Singleton Pattern
    public static SaveSystem instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Ini adalah fungsi yang akan dipanggil saatnya menyimpan
    public void SaveGameData()
    {
        // SAAT INI: Hanya placeholder
        Debug.Log("--- GAME DISIMPAN! ---");
        
        // 1. Ambil data dari sistem lain
        int currentDay = TimeSystem.instance.GetCurrentDay();
        int money = InventorySystem.instance.money;
        int rawFishCount = InventorySystem.instance.rawFishCount;
        int baitCount = InventorySystem.instance.baitCount;
        bool hasSmallBoat = InventorySystem.instance.hasSmallBoat;
        bool ownsBigBoat = InventorySystem.instance.ownsBigBoat;

        // 2. Simpan ke PlayerPrefs
        PlayerPrefs.SetInt("Save_CurrentDay", currentDay);
        PlayerPrefs.SetInt("Save_Money", money);
        PlayerPrefs.SetInt("Save_RawFishCount", rawFishCount);
        PlayerPrefs.SetInt("Save_BaitCount", baitCount);
        
        // PlayerPrefs tidak bisa simpan bool, jadi kita ubah ke int (1=true, 0=false)
        PlayerPrefs.SetInt("Save_HasSmallBoat", hasSmallBoat ? 1 : 0);
        PlayerPrefs.SetInt("Save_OwnsBigBoat", ownsBigBoat ? 1 : 0);

        // 3. Terapkan perubahan
        PlayerPrefs.Save();
    }

    public void LoadGameData()
    {
        // Cek dulu apakah ada data save (kita cek 'CurrentDay')
        if (PlayerPrefs.HasKey("Save_CurrentDay"))
        {
            Debug.Log("--- MEMUAT DATA GAME! ---");

            // 1. Baca data dari PlayerPrefs
            int currentDay = PlayerPrefs.GetInt("Save_CurrentDay");
            int money = PlayerPrefs.GetInt("Save_Money");
            int rawFishCount = PlayerPrefs.GetInt("Save_RawFishCount");
            int baitCount = PlayerPrefs.GetInt("Save_BaitCount");
            bool hasSmallBoat = PlayerPrefs.GetInt("Save_HasSmallBoat") == 1; // 1 == true
            bool ownsBigBoat = PlayerPrefs.GetInt("Save_OwnsBigBoat") == 1;

            // 2. Kirim data ke sistem yang relevan
            // PENTING: Kita perlu fungsi baru di TimeSystem/InventorySystem
            TimeSystem.instance.LoadDay(currentDay);
            InventorySystem.instance.LoadInventory(money, rawFishCount, baitCount, hasSmallBoat, ownsBigBoat);
        }
        else
        {
            Debug.Log("Tidak ada data save. Memulai game baru.");
        }
    }
}