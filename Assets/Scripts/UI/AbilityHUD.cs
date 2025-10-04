using UnityEngine;
using UnityEngine.UI; // Nécessaire pour les composants UI (Image, Text, etc.)

/// <summary>
/// Met à jour l'interface utilisateur en fonction des capacités actives du joueur.
/// </summary>
public class AbilityHUD : MonoBehaviour
{
    // Références à assigner dans l'Inspector
    [Header("Références")]
    [Tooltip("L'objet qui contient l'icône de Saut/Jetpack")]
    [SerializeField] private GameObject jumpIconContainer;
    [SerializeField] private GameObject jetpackIconContainer;
    
    // Le gestionnaire d'événements du joueur
    [SerializeField] private PlayerAbilityManager abilityManager; 
    
    // Vous pouvez également ajouter ici des références pour le Fuel Bar (Barre de Carburant)

    private void Start()
    {
        if (abilityManager == null)
        {
            Debug.LogError("AbilityHUD : Référence PlayerAbilityManager manquante. Assignez-la !");
            return;
        }

        // 1. Abonnement à l'événement de perte de capacité (le moment de la transition)
        abilityManager.OnJumpCapabilityLost += OnJumpLost; 
        
        // 2. Initialisation de l'état au démarrage du jeu
        UpdateHUD(abilityManager.CanJump, abilityManager.CanUseJetpack);
    }
    
    // Cette méthode est appelée AUTOMATIQUEMENT lorsque l'événement se déclenche
    private void OnJumpLost()
    {
        // La perte du saut implique l'activation du jetpack (selon votre logique)
        UpdateHUD(false, true); 
        
        // CONSEIL PRO : Vous pouvez ici jouer une animation ou un son de transition pour l'UI !
    }

    /// <summary>
    /// Met à jour l'affichage des icônes.
    /// </summary>
    private void UpdateHUD(bool canJump, bool canUseJetpack)
    {
        // On affiche soit l'un, soit l'autre.
        if (jumpIconContainer != null)
        {
            jumpIconContainer.SetActive(canJump);
        }

        if (jetpackIconContainer != null)
        {
            jetpackIconContainer.SetActive(canUseJetpack);
        }
    }
    
    private void OnDestroy()
    {
        // Se désabonner pour éviter les fuites de mémoire 
        if (abilityManager != null)
        {
            abilityManager.OnJumpCapabilityLost -= OnJumpLost;
        }
    }
}