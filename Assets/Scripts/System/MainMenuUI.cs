using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Hook this to your Main Menu canvas and assign the Continue button in the Inspector.
// It will disable + darken the continue button when there is no save data.
public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons (assign in Inspector)")]
    public Button continueButton;
    public Button newGameButton;

    [Header("Appearance")]
    public Color disabledColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    private Color continueImageOriginal;
    private Color continueTextOriginal;
    private Image continueImage;
    private TMPro.TextMeshProUGUI continueTMP;
    private UnityEngine.UI.Text continueText;

    void Awake()
    {
        if (continueButton != null)
        {
            continueImage = continueButton.GetComponent<Image>();
            if (continueImage != null) continueImageOriginal = continueImage.color;

            // Try TextMeshPro first
            continueTMP = continueButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (continueTMP != null)
            {
                continueTextOriginal = continueTMP.color;
            }
            else
            {
                // Fallback to legacy UI Text
                continueText = continueButton.GetComponentInChildren<UnityEngine.UI.Text>();
                if (continueText != null) continueTextOriginal = continueText.color;
            }
        }
    }

    void Start()
    {
        RefreshButtonState();
    }

    // Can be called from other scripts (e.g. after saving/deleting) to refresh state
    public void RefreshButtonState()
    {
        bool hasSave = SaveSystem.HasSaveData();

        if (continueButton != null)
        {
            continueButton.interactable = hasSave;

            if (continueImage != null)
            {
                continueImage.color = hasSave ? continueImageOriginal : disabledColor;
            }

            if (continueTMP != null)
            {
                continueTMP.color = hasSave ? continueTextOriginal : disabledColor;
            }
            else if (continueText != null)
            {
                continueText.color = hasSave ? continueTextOriginal : disabledColor;
            }
        }

        // Optional: keep New Game always interactable; we don't change it here
    }
}
