using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNavigator : MonoBehaviour
{
    public void NewGame()
    {
        // Hapus semua data save
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        // Load scene game baru
        SceneManager.LoadScene("In Game");
    }
    
    public void LoadGame()
    {
        // Cek apakah SaveSystem sudah ada
        if (SaveSystem.instance == null)
        {
            Debug.LogWarning("SaveSystem belum di-instantiate. Memulai game baru saja.");
            SceneManager.LoadScene("In Game");
            return;
        }
        
        SaveSystem.instance.LoadGameData();
        SceneManager.LoadScene("In Game");
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