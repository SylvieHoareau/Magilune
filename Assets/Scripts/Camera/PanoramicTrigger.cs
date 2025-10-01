using UnityEngine;

public class PanoramicTrigger : MonoBehaviour
{
     // Glissez-déposez l'objet CM_Main_Camera (qui a l'Animator) ici
    [SerializeField] private CameraManager cameraManager   ;

    private void OnTriggerEnter2D(Collider2D other)
    {

        // DEBUG : L'événement OnTriggerEnter2D a été déclenché
        Debug.Log("OnTriggerEnter2D déclenché avec : " + other.name);

        if (other.CompareTag("Player") && cameraManager != null)
        {
            cameraManager.SetPanoramicCameraActive();
            Debug.Log("CAMÉRA PANORAMIQUE : Vue panoramique activée.");
        }
        else if (cameraManager == null)
        {
            Debug.LogWarning($"Tag invalide : {other.gameObject.name} n'a pas le tag 'Player'.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && cameraManager != null)
        {
            cameraManager.SetFollowCameraActive();
            Debug.Log("CAMÉRA DE SUIVI : Vue de suivi activée.");
        }
    }
}
