using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class FishData {
    public string fishName;
    public FishSize sizeCategory; // Small, Medium, Big
    public Sprite fishIcon;       // Gambar ikan
    [Range(0, 100)] public float rarityChance; 
    public bool isUnlocked;       
}

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager instance;

    // Referensi ke Player untuk nge-pause
    private PlayerController playerController;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    [Header("Database Ikan")]
    public FishData[] fishDatabase; 

    [Header("UI Koleksi")]
    public GameObject collectionPanel;
    public Image[] fishSlots; 
    
    [Header("UI Statistik")]
    public TextMeshProUGUI smallCountText;
    public TextMeshProUGUI mediumCountText;
    public TextMeshProUGUI bigCountText;

    void Start() {
        collectionPanel.SetActive(false);
        
        // Cari PlayerController di Scene saat mulai
        playerController = FindObjectOfType<PlayerController>();
        
        UpdateCollectionUI();
    }

    // --- LOGIKA MEMBUKA KOLEKSI (DIPERBARUI) ---
    public void ToggleCollectionPanel() {
        bool isActive = collectionPanel.activeSelf;
        
        // Balik statusnya (jika aktif jadi mati, jika mati jadi aktif)
        bool newStatus = !isActive;
        
        collectionPanel.SetActive(newStatus);

        // Update visual jika dibuka
        if (newStatus) {
            UpdateCollectionUI();
        }

        // --- PAUSE / UNPAUSE PLAYER ---
        // Jika newStatus == true (Panel Buka), maka Game Pause
        if (playerController == null) {
            playerController = FindObjectOfType<PlayerController>();
        }

        if (playerController != null) {
            playerController.SetUIState(newStatus);
        }
    }

    // Update tampilan icon (Gelap/Terang) dan jumlah teks
    public void UpdateCollectionUI() {
        // 1. Update Slot Gambar
        for (int i = 0; i < fishDatabase.Length; i++) {
            if (i < fishSlots.Length) {
                fishSlots[i].sprite = fishDatabase[i].fishIcon;

                if (fishDatabase[i].isUnlocked) {
                    fishSlots[i].color = Color.white; 
                } else {
                    fishSlots[i].color = Color.black; 
                }
            }
        }

        // 2. Update Text Jumlah
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
    public FishData GetRandomFish(FishSize size) {
        int startIndex = 0;
        int endIndex = 0;

        switch (size) {
            case FishSize.Small: startIndex = 0; endIndex = 2; break;
            case FishSize.Medium: startIndex = 3; endIndex = 5; break;
            case FishSize.Big: startIndex = 6; endIndex = 8; break;
        }

        float roll = Random.Range(0f, 100f);
        float cumulative = 0f;

        for (int i = startIndex; i <= endIndex; i++) {
            cumulative += fishDatabase[i].rarityChance;
            if (roll <= cumulative) {
                return fishDatabase[i];
            }
        }
        
        return fishDatabase[startIndex];
    }

    public bool UnlockFish(string name) {
        foreach (var fish in fishDatabase) {
            if (fish.fishName == name) {
                if (!fish.isUnlocked) {
                    fish.isUnlocked = true;
                    UpdateCollectionUI(); 
                    return true; 
                }
                return false; 
            }
        }
        return false;
    }
}