using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Active une Cinemachine Virtual Camera à haute priorité
/// pour un plan large ou un moment contemplatif
/// </summary>
public class CameraSwitchTrigger : MonoBehaviour
{
    [Tooltip("La CM vcam à haute priorité pour le plan large.")]
    [SerializeField] private GameObject panoramicCameraObject;

    // Réfrence privée pour stocker le composant réel
    private CinemachineCamera panoramicVcam; 

    [Tooltip("La priorité à laquelle la camréa panomarique doit basculer")]
    [SerializeField] private int activePriority = 20;

    private int originalPriority;

    void Awake()
    {
        // Vérification 1 : Récupération et vérification du composant Vcam
        if (panoramicCameraObject == null)
        {
            Debug.LogError($"[CameraSwitchTrigger] ERREUR CRITIQUE : Le champ 'Panoramic Camera Object' n'est pas assigné sur {gameObject.name}. Veuillez l'assigner dans l'Inspector.", this);
            return;
        }

        // Vérification 2 : Le composant CinemachineVirtualCamera est-il présent sur cet objet ?
        panoramicVcam = panoramicCameraObject.GetComponent<CinemachineCamera>();

        if (panoramicVcam == null)
        {
            // On signale l'erreur mais on ne risque plus le NullReference.
            Debug.LogError("Le GameObject assigné ne contient pas de composant CinemachineVirtualCamera !");
            // On continue l'exécution (mais les méthodes OnTrigger ne feront rien car panoramicVcam est null)
            return;
        }

        // Une fois que le composant est valide, on stocke la priorité originale de la caméra
        originalPriority = panoramicVcam.Priority;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // On s'assure que la Vcam est bien initialisée
        if (panoramicVcam == null) return; 

        // Vérifie si l'objet entrant est le joueur (via son Tag ou une Layer)
        if (other.CompareTag("Player"))
        {
            // Active le plan panoramique en augmentant sa priorité
            panoramicVcam.Priority = activePriority;
            Debug.Log($"Passage à la vue panoramique. Priorité: {activePriority}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (panoramicVcam == null) return; 

        if (other.CompareTag("Player"))
        {
            // Retourne à la caméra par défaut en restaurant la priorité
            panoramicVcam.Priority = originalPriority;
            Debug.Log("Retour à la vue normale.");
        }
    }
}
