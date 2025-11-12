using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How fast the cloud moves from side to side.")]
    public float speed = 1.0f;

    [Tooltip("How far (in units) the cloud will move from its starting point in ONE direction.")]
    public float moveDistance = 5.0f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {

        float offset = Mathf.PingPong(Time.time * speed, moveDistance * 2) - moveDistance;
        float newXPosition = startPosition.x + offset;
        transform.position = new Vector3(newXPosition, startPosition.y, startPosition.z);
    }
}