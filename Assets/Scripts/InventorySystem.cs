using UnityEngine;
using TMPro;

// Kita definisikan tipe ikan di sini agar bisa diakses semua script
public enum FishSize { Small, Medium, Big }

public class InventorySystem : MonoBehaviour {
    public static InventorySystem instance;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    // --- Data Inventory ---
    public int baitCount = 5;
    
    // Variabel Spesifik per ukuran
    public int smallFishCount = 0;
    public int mediumFishCount = 0;
    public int bigFishCount = 0;
    
    public int money = 0;
    public bool hasSmallBoat = false;
    public bool ownsBigBoat = false;

    // --- Harga Jual Ikan ---
    [Header("Harga Jual Ikan")]
    public int smallFishPrice = 10;
    public int mediumFishPrice = 30;
    public int bigFishPrice = 100;

    [Header("Referensi UI (Utama)")]
    public TextMeshProUGUI baitText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI rawFishText; // Menampilkan TOTAL ikan

    [Header("Referensi UI (Detail - Opsional)")]
    // Masukkan Text UI ke sini jika ingin menampilkan jumlah spesifik di layar
    public TextMeshProUGUI smallFishText;
    public TextMeshProUGUI mediumFishText;
    public TextMeshProUGUI bigFishText;

    void Start() {
        UpdateInventoryUI();
    }

    public void LoadInventory(int lMoney, int sFish, int mFish, int bFish, int lBait, bool lSmallBoat, bool lBigBoat)
    {
        money = lMoney;
        smallFishCount = sFish;
        mediumFishCount = mFish;
        bigFishCount = bFish;
        baitCount = lBait;
        hasSmallBoat = lSmallBoat;
        ownsBigBoat = lBigBoat;
        UpdateInventoryUI();
    }

    // --- FUNGSI UPDATE UI (DIPERBARUI) ---
    public void UpdateInventoryUI() {
        // 1. Update Umpan & Uang
        if (baitText != null) baitText.text = baitCount.ToString();
        if (moneyText != null) moneyText.text = money.ToString();

        // 2. Update Total Ikan
        int totalFish = smallFishCount + mediumFishCount + bigFishCount;
        if (rawFishText != null) {
            rawFishText.text = totalFish.ToString();
        }

        // 3. Update Detail Ikan (Jika UI-nya dipasang)
        if (smallFishText != null) smallFishText.text = smallFishCount.ToString();
        if (mediumFishText != null) mediumFishText.text = mediumFishCount.ToString();
        if (bigFishText != null) bigFishText.text = bigFishCount.ToString();
    }

    // --- Fungsi UMPAN ---
    public bool HasBait(int amount) {
        return baitCount >= amount;
    }

    public void AddBait(int amount) {
        baitCount += amount;
        UpdateInventoryUI();
    }

    public void UseBait(int amount) {
        baitCount -= amount;
        if (baitCount < 0) baitCount = 0;
        UpdateInventoryUI();
    }

    // --- Fungsi IKAN ---
    public void AddFish(FishSize size, int amount) {
        switch (size) {
            case FishSize.Small: smallFishCount += amount; break;
            case FishSize.Medium: mediumFishCount += amount; break;
            case FishSize.Big: bigFishCount += amount; break;
        }
        UpdateInventoryUI(); // Refresh UI
    }

    public int SellAllFish() {
        int totalEarnings = 0;

        if (smallFishCount > 0) {
            totalEarnings += (smallFishCount * smallFishPrice);
            smallFishCount = 0;
        }
        if (mediumFishCount > 0) {
            totalEarnings += (mediumFishCount * mediumFishPrice);
            mediumFishCount = 0;
        }
        if (bigFishCount > 0) {
            totalEarnings += (bigFishCount * bigFishPrice);
            bigFishCount = 0;
        }

        if (totalEarnings > 0) {
            AddMoney(totalEarnings);
        }
        
        UpdateInventoryUI(); // Refresh UI setelah jual
        return totalEarnings;
    }

    // --- Fungsi UANG ---
    public void AddMoney(int amount) {
        money += amount;
        UpdateInventoryUI();
    }

    public bool HasMoney(int amount) {
        return money >= amount;
    }

    public void SpendMoney(int amount) {
        if (HasMoney(amount)) {
            money -= amount;
            UpdateInventoryUI();
        }
    }

    public bool PurchaseItem(int price) {
        if (HasMoney(price)) {
            SpendMoney(price);
            return true;
        }
        return false;
    }

    public int GetTotalFishCount()
    {
        return smallFishCount + mediumFishCount + bigFishCount;
    }
}