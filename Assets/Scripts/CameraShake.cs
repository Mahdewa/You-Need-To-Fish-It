using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;

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

    // Method untuk trigger camera shake
    public static void Shake(float duration = 0.2f, float magnitude = 0.3f)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.ShakeCoroutine(duration, magnitude));
        }
    }

    // Coroutine untuk camera shake
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Generate random offset
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            // Apply shake
            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Kembalikan ke posisi original
        transform.localPosition = originalPosition;

        Debug.Log("Camera shake selesai!");
    }
}
