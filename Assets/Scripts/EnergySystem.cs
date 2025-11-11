using UnityEngine;
using UnityEngine.UI; // Anda akan butuh ini nanti untuk Slider (Energy Bar)

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

    void Start() {
        currentEnergy = maxEnergy; // Mulai dengan energi penuh
        UpdateEnergyBar();
    }

    public bool HasEnoughEnergy(float amount) {
        return currentEnergy >= amount;
    }

    public void UseEnergy(float amount) {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
        Debug.Log($"Pakai {amount} Energi. Sisa: {currentEnergy}");
        UpdateEnergyBar();
    }

    public void RestoreEnergy(float amount) {
        currentEnergy += amount;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        Debug.Log($"Pulih {amount} Energi. Total: {currentEnergy}");
        UpdateEnergyBar();
    }

    public void RestoreAllEnergy() {
        currentEnergy = maxEnergy;
        Debug.Log("Energi pulih penuh!");
        UpdateEnergyBar();
    }

    public void UpdateEnergyBar() {
        if (energyBar != null) {
            energyBar.value = currentEnergy / maxEnergy;
        }
    }
}