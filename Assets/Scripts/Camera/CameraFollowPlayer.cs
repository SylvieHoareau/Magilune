using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public Transform player;  // assigne ton Player ici
    public Vector3 offset;    // décalage par rapport au player
    public float smoothSpeed = 0.125f; // fluidité du suivi

    void LateUpdate()
    {
        Vector3 targetPos = player.position + offset;
        Vector3 smoothedPos = Vector3.Lerp(transform.position, targetPos, smoothSpeed);
        transform.position = new Vector3(smoothedPos.x, smoothedPos.y, transform.position.z);
    }
}
