using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneAudioManager : MonoBehaviour
{
    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Cek musik apa yang seharusnya dimainkan di scene ini
        switch (currentScene)
        {
            case "MainMenu":
                AudioManager.Instance.PlayBGM("Main Menu");
                break;

            case "Prologue":
                AudioManager.Instance.PlayBGM("Prologue");
                break;

            case "In Game":
                AudioManager.Instance.PlayBGM("In Game");
                break;

            case "Settings":
                // Settings scene tetap putar musik dari scene sebelumnya
                AudioManager.Instance.PlayBGM("Main Menu");
                break;

            case "LoadingScene":
                AudioManager.Instance.PlayBGM("Loading");
                break;

            case "Epilogue":
                AudioManager.Instance.PlayBGM("Epilogue");
                break;

            default:
                Debug.LogWarning($"Scene '{currentScene}' tidak memiliki musik yang terdefinisi");
                break;
        }

        Debug.Log($"SceneAudioManager: Musik di-set untuk scene '{currentScene}'");
    }
}
