using UnityEngine;

public class cameraMovement : MonoBehaviour
{
    [SerializeField] private Transform target; // Reference ke karakter yang akan diikuti
    [SerializeField] private float smoothSpeed = 0.125f; // Kecepatan smooth movement kamera
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); // Offset kamera dari karakter

    void LateUpdate()
    {
        if (target != null)
        {
            // Menghitung posisi yang diinginkan
            Vector3 desiredPosition = target.position + offset;
            
            // Smoothly menggerakkan kamera ke posisi yang diinginkan
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}