using UnityEngine;
using TMPro;

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
    public int rawFishCount = 0;
    public int money = 0;
    public bool hasSmallBoat = false;
    public bool ownsBigBoat = false;

    [Header("Referensi UI")]
    public TextMeshProUGUI baitText;
    public TextMeshProUGUI rawFishText;
    public TextMeshProUGUI moneyText;

    void Start() {
        UpdateInventoryUI();
    }

    public void LoadInventory(int loadedMoney, bool loadedSmallBoat, bool loadedBigBoat)
    {
        money = loadedMoney;
        hasSmallBoat = loadedSmallBoat;
        ownsBigBoat = loadedBigBoat;

        // Update UI setelah me-load
        UpdateInventoryUI();
    }

    public void UpdateInventoryUI() {
        if (baitText != null) 
            baitText.text = baitCount.ToString();
            
        if (rawFishText != null)
            rawFishText.text = rawFishCount.ToString();
            
        if (moneyText != null)
            moneyText.text = money.ToString();
    }

    // --- Fungsi untuk UMPAN ---
    public bool HasBait(int amount) {
        return baitCount >= amount;
    }

    public void AddBait(int amount) {
        baitCount += amount;
        Debug.Log($"Dapat {amount} Umpan! Total: {baitCount}");
        UpdateInventoryUI();
    }

    public void UseBait(int amount) {
        baitCount -= amount;
        if (baitCount < 0) baitCount = 0;
        Debug.Log($"Pakai {amount} Umpan. Sisa: {baitCount}");
        UpdateInventoryUI();
    }

    // --- Fungsi untuk IKAN ---
    public void AddRawFish(int amount) {
        rawFishCount += amount;
        Debug.Log($"Dapat {amount} Ikan Mentah! Total: {rawFishCount}");
        UpdateInventoryUI();
    }

    public bool HasRawFish(int amount) {
        return rawFishCount >= amount;
    }

    public void UseRawFish(int amount) {
        if (HasRawFish(amount)) {
            rawFishCount -= amount;
            Debug.Log($"Menjual {amount} ikan. Sisa ikan mentah: {rawFishCount}");
            UpdateInventoryUI();
        }
    }

    // --- FUNGSI UNTUK UANG ---
    public void AddMoney(int amount) {
        money += amount;
        Debug.Log($"Dapat {amount} uang. Total Uang: {money}");
        UpdateInventoryUI();
    }

    public bool HasMoney(int amount) {
        return money >= amount;
    }

    public void SpendMoney(int amount) {
        if (HasMoney(amount))
        {
            money -= amount;
            Debug.Log($"Membelanjakan {amount} uang. Sisa Uang: {money}");
            UpdateInventoryUI();
        }
    }

    public bool PurchaseItem(int price) {
        if (HasMoney(price)) {
            SpendMoney(price);
            return true;
        }
        else {
            Debug.Log($"Uang tidak cukup! Butuh {price}, kamu punya {money}");
            return false;
        }
    }
}