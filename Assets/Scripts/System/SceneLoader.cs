using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video; // Diperlukan untuk mengakses VideoPlayer

public class SceneLoader : MonoBehaviour
{
    // Variabel statis untuk menyimpan nama scene yang ingin kita tuju
    public static string sceneToLoad;

    [Header("Pengaturan Tambahan")]
    [Tooltip("Durasi minimal loading screen tampil (detik), agar tidak terlalu cepat hilang.")]
    [SerializeField] private float minLoadingTime = 3f;

    void Start()
    {
        // Mulai proses loading di latar belakang
        StartCoroutine(LoadSceneAsyncRoutine());
    }

    private IEnumerator LoadSceneAsyncRoutine()
    {
        // Mulai memuat scene yang dituju secara asinkron
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        
        // Tahan agar scene tidak langsung aktif setelah selesai 90%
        operation.allowSceneActivation = false;

        float timer = 0f;

        // Tunggu hingga proses loading di latar belakang hampir selesai
        // dan durasi minimal penantian juga sudah terpenuhi
        while (!operation.isDone || timer < minLoadingTime)
        {
            timer += Time.deltaTime;
            
            // Saat loading mencapai 90%, Unity akan berhenti sejenak
            // Kita bisa menggunakan ini untuk memastikan durasi minimal terpenuhi
            if (operation.progress >= 0.9f && timer >= minLoadingTime)
            {
                // Izinkan scene untuk aktif dan ditampilkan
                operation.allowSceneActivation = true;
            }
            
            yield return null;
        }
    }
}