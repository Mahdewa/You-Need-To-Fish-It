using UnityEngine;

public class CameraOffsetTrigger : MonoBehaviour
{
    [Header("Camera Offset Type")]
    [SerializeField] private CameraOffsetType offsetType = CameraOffsetType.Left;
    
    [Header("Custom Offset (jika Type = Custom)")]
    [SerializeField] private Vector3 customOffset = new Vector3(-5f, 0, -10f);

    private cameraMovement cameraScript;
    private bool hasTriggered = false;

    public enum CameraOffsetType
    {
        Left,
        Right,
        Custom
    }

    private void Start()
    {
        // Cari script cameraMovement
        cameraScript = FindObjectOfType<cameraMovement>();
        if (cameraScript == null)
        {
            Debug.LogWarning("cameraMovement script tidak ditemukan di scene!");
        }

        // Pastikan collider ini adalah trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah yang masuk adalah player
        if (other.CompareTag("Player") && cameraScript != null && !hasTriggered)
        {
            hasTriggered = true;

            // Ubah offset kamera sesuai type
            switch (offsetType)
            {
                case CameraOffsetType.Left:
                    cameraScript.SetLeftCameraOffset();
                    break;
                case CameraOffsetType.Right:
                    cameraScript.SetCustomCameraOffset(new Vector3(5f, 0, -10f));
                    break;
                case CameraOffsetType.Custom:
                    cameraScript.SetCustomCameraOffset(customOffset);
                    break;
            }

            Debug.Log($"Player entered camera trigger - Offset: {offsetType}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Cek apakah yang keluar adalah player
        if (other.CompareTag("Player") && cameraScript != null && hasTriggered)
        {
            hasTriggered = false;

            // Kembalikan offset kamera ke normal
            cameraScript.SetNormalCameraOffset();

            Debug.Log("Player exited camera trigger - Offset reset to NORMAL");
        }
    }
}

