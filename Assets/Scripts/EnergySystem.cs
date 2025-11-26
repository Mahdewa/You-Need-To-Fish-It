using UnityEngine;
using UnityEngine.UI; 
using TMPro; // PENTING: Tambahkan ini untuk akses TextMeshPro

public class EnergySystem : MonoBehaviour
{
    // Singleton Pattern
    public static EnergySystem instance;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    // --- Data Energi ---
    public float maxEnergy = 100f;
    public float currentEnergy;

    [Header("UI Referensi")]
    public Slider energyBar;
    
    // VARIABEL BARU: Referensi untuk teks persentase
    public TextMeshProUGUI energyPercentText; 

    void Start() {
        // Load data atau set default
        if (currentEnergy <= 0) currentEnergy = maxEnergy; 
        
        UpdateEnergyBar();
    }

    public bool HasEnoughEnergy(float amount) {
        return currentEnergy >= amount;
    }

    public void UseEnergy(float amount) {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
        UpdateEnergyBar();
    }

    public void RestoreEnergy(float amount) {
        currentEnergy += amount;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        UpdateEnergyBar();
    }

    public void AddEnergy(float amount)
    {
        currentEnergy += amount;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy; 
        UpdateEnergyBar(); 
    }

    public void RestoreAllEnergy() {
        currentEnergy = maxEnergy;
        UpdateEnergyBar();
    }

    // --- FUNGSI UPDATE UI (DIPERBARUI) ---
    public void UpdateEnergyBar() {
        // 1. Update Slider (Bar)
        if (energyBar != null) {
            energyBar.value = currentEnergy / maxEnergy;
        }

        // 2. Update Teks Persentase (BARU)
        if (energyPercentText != null) {
            // Rumus: (Sekarang / Maksimal) * 100
            float percent = (currentEnergy / maxEnergy) * 100f;
            
            // Format "F0" artinya bulat tanpa desimal (misal: 95%)
            // Kalau mau ada koma (95.5%), ganti jadi "F1"
            energyPercentText.text = $"{percent:F0}%";
        }
    }
}