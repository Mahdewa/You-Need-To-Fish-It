using UnityEngine;

public class cameraMovement : MonoBehaviour
{
    [SerializeField] private Transform target; // Reference ke karakter yang akan diikuti
    [SerializeField] private float smoothSpeed = 0.125f; // Kecepatan smooth movement kamera
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); // Offset kamera dari karakter
    
    [Header("Camera Offset Settings")]
    [SerializeField] private Vector3 normalOffset = new Vector3(0, 0, -10); // Offset normal
    [SerializeField] private Vector3 leftOffset = new Vector3(-5, 0, -10); // Offset ke kiri
    [SerializeField] private float offsetSmoothSpeed = 0.05f; // Kecepatan smooth untuk perubahan offset
    
    private Vector3 targetOffset; // Offset yang sedang ditargetkan
    private Vector3 currentOffset; // Offset yang sedang aktif (untuk interpolation)

    private void Start()
    {
        // Inisialisasi target offset
        targetOffset = offset;
        currentOffset = offset;
        normalOffset = offset; // Set normal offset sesuai offset default
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Smoothly interpolate offset dari currentOffset ke targetOffset
            currentOffset = Vector3.Lerp(currentOffset, targetOffset, offsetSmoothSpeed);
            
            // Menghitung posisi yang diinginkan dengan current offset
            Vector3 desiredPosition = target.position + currentOffset;
            
            // Smoothly menggerakkan kamera ke posisi yang diinginkan
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }

    // --- Method untuk mengubah offset kamera ---
    public void SetLeftCameraOffset()
    {
        targetOffset = leftOffset;
        Debug.Log("Camera offset changed to LEFT");
    }

    // Method untuk mengembalikan offset normal
    public void SetNormalCameraOffset()
    {
        targetOffset = normalOffset;
        Debug.Log("Camera offset changed to NORMAL");
    }

    // Method untuk set custom offset
    public void SetCustomCameraOffset(Vector3 newOffset)
    {
        targetOffset = newOffset;
        Debug.Log($"Camera offset changed to: {newOffset}");
    }
}