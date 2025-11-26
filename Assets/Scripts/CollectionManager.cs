using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class FishData {
    public string fishName;
    public FishSize sizeCategory; // Small, Medium, Big
    public Sprite fishIcon;       // Gambar ikan
    [Range(0, 100)] public float rarityChance; // Persentase muncul (misal 60, 30, 10)
    public bool isUnlocked;       // Status apakah sudah pernah ditangkap
}

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager instance;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    [Header("Database Ikan (Isi 9 Ikan di Inspector)")]
    public FishData[] fishDatabase; 
    // Urutan (Disarankan): 0-2 (Small), 3-5 (Medium), 6-8 (Big)

    [Header("UI Koleksi")]
    public GameObject collectionPanel;
    public Image[] fishSlots; // 9 Slot Gambar di Panel
    
    [Header("UI Statistik")]
    public TextMeshProUGUI smallCountText;
    public TextMeshProUGUI mediumCountText;
    public TextMeshProUGUI bigCountText;

    void Start() {
        collectionPanel.SetActive(false);
        UpdateCollectionUI();
    }

    // --- LOGIKA MEMBUKA KOLEKSI ---
    public void ToggleCollectionPanel() {
        bool isActive = collectionPanel.activeSelf;
        collectionPanel.SetActive(!isActive);

        if (!isActive) {
            // Saat dibuka, update tampilannya
            UpdateCollectionUI();
        }
    }

    // Update tampilan icon (Gelap/Terang) dan jumlah teks
    public void UpdateCollectionUI() {
        // 1. Update Slot Gambar
        for (int i = 0; i < fishDatabase.Length; i++) {
            if (i < fishSlots.Length) {
                // Set Sprite
                fishSlots[i].sprite = fishDatabase[i].fishIcon;

                // Atur Warna: Putih (Normal) jika unlocked, Hitam/Gelap jika belum
                if (fishDatabase[i].isUnlocked) {
                    fishSlots[i].color = Color.white; 
                } else {
                    fishSlots[i].color = Color.black; // Atau color gelap transparan
                }
            }
        }

        // 2. Update Text Jumlah (Ambil dari Inventory)
        if (InventorySystem.instance != null) {
            if (smallCountText != null) 
                smallCountText.text = "" + InventorySystem.instance.smallFishCount;
            
            if (mediumCountText != null) 
                mediumCountText.text = "" + InventorySystem.instance.mediumFishCount;
            
            if (bigCountText != null) 
                bigCountText.text = "" + InventorySystem.instance.bigFishCount;
        }
    }

    // --- LOGIKA MENDAPATKAN IKAN ---
    
    // Fungsi ini dipanggil PlayerController untuk menentukan ikan spesifik apa yang didapat
    public FishData GetRandomFish(FishSize size) {
        // Filter database berdasarkan ukuran
        // Asumsi urutan array: 0-2 Small, 3-5 Medium, 6-8 Big
        
        int startIndex = 0;
        int endIndex = 0;

        switch (size) {
            case FishSize.Small: startIndex = 0; endIndex = 2; break;
            case FishSize.Medium: startIndex = 3; endIndex = 5; break;
            case FishSize.Big: startIndex = 6; endIndex = 8; break;
        }

        // Roll Random (0 - 100)
        float roll = Random.Range(0f, 100f);
        float cumulative = 0f;

        // Loop di kategori tersebut untuk cek rarity
        for (int i = startIndex; i <= endIndex; i++) {
            cumulative += fishDatabase[i].rarityChance;
            if (roll <= cumulative) {
                return fishDatabase[i];
            }
        }
        
        // Fallback (kembalikan yang pertama di kategori itu jika hitungan meleset)
        return fishDatabase[startIndex];
    }

    // Fungsi untuk Unlock (Return true jika ini ikan BARU)
    public bool UnlockFish(string name) {
        foreach (var fish in fishDatabase) {
            if (fish.fishName == name) {
                if (!fish.isUnlocked) {
                    fish.isUnlocked = true;
                    UpdateCollectionUI(); // Refresh jika panel sedang terbuka
                    return true; // "NEW FISH!"
                }
                return false; // Sudah pernah punya
            }
        }
        return false;
    }
}