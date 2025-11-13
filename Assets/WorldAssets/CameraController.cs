using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    private Vector3 offset = new Vector3(0f, 1f, -10f);
    private Vector3 velocity = Vector3.zero;

    public Transform target;
    public float smoothTime = 0.2f;

    [Header("Camera Bounds (optional)")]
    [Tooltip("Enable to constrain camera at map edges")]
    public bool useBounds = false;
    
    [Tooltip("Minimum X position for camera")]
    public float minX = -50f;
    
    [Tooltip("Maximum X position for camera")]
    public float maxX = 50f;
    
    [Tooltip("Minimum Y position for camera")]
    public float minY = -50f;
    
    [Tooltip("Maximum Y position for camera")]
    public float maxY = 50f;


    // LateUpdate is called once per frame, after other stuff
    void LateUpdate()
    {
        Vector3 targetPosition = target.position + offset;
        Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // Clamp camera position to bounds if enabled
        if (useBounds)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        }

        transform.position = newPosition;
    }
}
