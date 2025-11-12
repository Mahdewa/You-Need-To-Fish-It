using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNavigator : MonoBehaviour
{
    public void NewGame()
    {
        // Hapus semua data save
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        FadeManager.DestroyFadeManager();

        // Load scene game baru
        SceneLoader.sceneToLoad = "Prologue";
        SceneManager.LoadScene("LoadingScene");
    }
    
    public void LoadGame()
    {
        // Cek apakah SaveSystem sudah ada
        if (SaveSystem.instance == null)
        {
            FadeManager.DestroyFadeManager();
            Debug.LogWarning("SaveSystem belum di-instantiate. Memulai game baru saja.");
            SceneLoader.sceneToLoad = "In Game";
            SceneManager.LoadScene("LoadingScene");
            return;
        }
        
        SaveSystem.instance.LoadGameData();
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void BackToMenuWithLoading()
    {
        SceneLoader.sceneToLoad = "MainMenu";
        SceneManager.LoadScene("LoadingScene");
    }


    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}