using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;
    
    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;
    
    private bool isPaused = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Pastikan pause panel inactive di awal
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Tekan ESC untuk pause/resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f; // Freeze game

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("button");
        }
        else
        {
            Debug.LogError("AudioManager tidak ditemukan! Pastikan scene Main Menu dijalankan pertama kali.");
        }
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        
        Debug.Log("Game Paused!");
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume game

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("button");
        }
        else
        {
            Debug.LogError("AudioManager tidak ditemukan! Pastikan scene Main Menu dijalankan pertama kali.");
        }
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        
        Debug.Log("Game Resumed!");
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    // Method untuk tombol Resume di panel
    public void OnResumeButtonClicked()
    {
        Resume();
    }

    // Method untuk tombol Quit to Menu di panel
    public void OnQuitToMenuButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("button");
        }
        else
        {
            Debug.LogError("AudioManager tidak ditemukan! Pastikan scene Main Menu dijalankan pertama kali.");
        }
        
        Time.timeScale = 1f; // Kembalikan time scale ke normal
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
