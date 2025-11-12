using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EpilogueManager : MonoBehaviour
{
    void Start()
    {
        // Fade in dari black saat masuk scene
        StartCoroutine(FadeInEpilogue());
    }

    private IEnumerator FadeInEpilogue()
    {
        // Fade from black selama 2 detik
        yield return StartCoroutine(FadeManager.FadeFromBlack(2f));
    }

    void Update()
    {
        // Tekan ESC atau ENTER untuk kembali ke MainMenu
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
        {
            BackToMainMenu();
        }
    }

    public void BackToMainMenu()
    {
        // Fade to black sebelum pindah scene
        StartCoroutine(FadeAndLoadMenu());
    }

    private IEnumerator FadeAndLoadMenu()
    {
        // Fade to black
        yield return StartCoroutine(FadeManager.FadeToBlack(1f));
        
        // Hapus save ketika pemain telah menyelesaikan game
        SaveSystem.DeleteAllSaveData();

        // Load MainMenu
        SceneLoader.sceneToLoad = "MainMenu";
        SceneManager.LoadScene("LoadingScene");
    }
}
