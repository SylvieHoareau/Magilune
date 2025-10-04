using UnityEngine;

public class PanoramicTrigger : MonoBehaviour
{
     // Glissez-déposez l'objet CM_Main_Camera (qui a l'Animator) ici
    [SerializeField] private CameraManager cameraManager   ;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // DEBUG : On enregistre que le trigger a été atteint (étape 1 de debug)
        Debug.Log("OnTriggerEnter2D déclenché avec : " + other.name);

        // Vérifier si c'est le joueur
        if (!other.CompareTag("Player"))
        {
            Debug.Log("PanoramicTrigger : Déclencheur ignoré. L'objet n'est pas le 'Player'.");
            // C'est un objet qui n'est pas le joueur, on s'arrête.
            return;
        }

        // Vérifier si le CameraManager est assigné
        if (cameraManager == null)
        {
            Debug.LogError("PanoramicTrigger : Référence CameraManager manquante ! Veuillez l'assigner dans l'Inspector.");
            return;
        }

        // Déclenchement de la fonctionnalité
        cameraManager.SetPanoramicCameraActive();
        Debug.Log("CAMÉRA PANORAMIQUE : Vue panoramique activée.");
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
