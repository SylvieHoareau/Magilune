using UnityEngine;

/// <summary>
/// Déclenche le changement de caméra à l'entrée et à la sortie du joueur.
/// </summary>
public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private bool activatePanoramicOnEnter = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // S'assurer que c'est bien le joueur qui entre dans le trigger
        if (other.CompareTag("Player"))
        {
            if (activatePanoramicOnEnter)
            {
                cameraManager.SetPanoramicCameraActive();
            }
            else
            {
                // Utile si le trigger doit juste forcer la caméra de suivi
                cameraManager.SetFollowCameraActive();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Retourne toujours à la caméra de suivi après un moment contemplatif
            cameraManager.SetFollowCameraActive();
        }
    }
}