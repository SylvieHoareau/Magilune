using UnityEngine;
using Unity.Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineCamera followCam;
    public CinemachineCamera contemplativeCam;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            followCam.Priority = 10;
            contemplativeCam.Priority = 20;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            followCam.Priority = 20;
            contemplativeCam.Priority = 10;
        }
    }
}
