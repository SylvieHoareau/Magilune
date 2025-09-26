using UnityEngine;

public class CameraSync : MonoBehaviour
{
    public Transform targetCamera; // assigner Camera Follow ou Panoramic ici

    void LateUpdate()
    {
        transform.position = new Vector3(
            targetCamera.position.x, 
            targetCamera.position.y, 
            transform.position.z // garde la profondeur de Main Camera
        );
    }
}

