using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Gère la bascule entre les Virtual Cameras (Follow et Panoramique)
/// en ajustant leur priorité. Modulaire pour l'ajout futur d'autres vues.
/// </summary>
public class CameraManager : MonoBehaviour
{
    [Header("Cameras")]
    [Tooltip("Caméra principale, prioritaire par défaut (ex: 15)")]
    [SerializeField] private CinemachineCamera followCamera;
    
    [Tooltip("Caméra panoramique pour les moments contemplatifs (ex: 10)")]
    [SerializeField] private CinemachineCamera panoramicCamera;

    // Constantes pour la priorité
    private const int ActivePriority = 15; // Devrait correspondre à la priorité de 'followCamera'
    private const int InactivePriority = 10; // Devrait correspondre à la priorité de 'panoramicCamera'

    private void Start()
    {
        // S'assurer que la caméra de suivi est active au démarrage
        SetFollowCameraActive();
    }

    /// <summary>
    /// Active la caméra de suivi du joueur (haute priorité).
    /// </summary>
    public void SetFollowCameraActive()
    {
        followCamera.Priority = ActivePriority;
        panoramicCamera.Priority = InactivePriority;
        Debug.Log("Caméra : Suivi du Joueur activée.");
    }

    /// <summary>
    /// Active la caméra panoramique (haute priorité pour la bascule).
    /// </summary>
    public void SetPanoramicCameraActive()
    {
        panoramicCamera.Priority = ActivePriority;
        followCamera.Priority = InactivePriority;
        Debug.Log("Caméra : Vue Panoramique activée.");
    }

    // Pro-Tip: Vous pouvez appeler ces méthodes via un Trigger Volume
    // dans votre niveau pour déclencher les moments contemplatifs.
}