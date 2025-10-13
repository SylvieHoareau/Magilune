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
    [SerializeField] private GameObject grapplingIconContainer;
    [SerializeField] private GameObject climbingIconContainer;
    [SerializeField] private GameObject jetpackIconContainer;
    // [Tooltip("Le Slider (barre de progression) du carburant du Jetpack.")]
    // [SerializeField] private Slider jetpackFuelSlider; 
    
    // Le gestionnaire d'événements du joueur
    [SerializeField] private PlayerAbilityManager abilityManager;

    // Référence au module Jetpack pour accéder au ratio de carburant (NÉCESSAIRE)
    private JetpackAbility jetpackAbility;

    private void Start()
    {
        // Récupérer la référence au JetpackAbility
        if (abilityManager != null)
        {
            jetpackAbility = abilityManager.GetComponent<JetpackAbility>();
            Debug.LogError("AbilityHUD : Référence PlayerAbilityManager manquante. Assignez-la !");
            return;
        }

        // Vérification des références critiques
        if (abilityManager == null || jetpackAbility == null)
        {
            Debug.LogError("AbilityHUD : Références PlayerAbilityManager ou JetpackAbility manquantes. Assignez-les !");
            return;
        }

        // Abonnement à l'événement de perte de capacité (le moment de la transition)
        abilityManager.OnJumpCapabilityLost += OnJumpLost;

        // Initialisation de l'état au démarrage du jeu
        UpdateHUD(abilityManager.CanJump, abilityManager.CanUseJetpack);
        
        // Initialiser le slider à plein au démarrage (si le jetpack est actif)
        // if (jetpackFuelSlider != null)
        // {
        //     jetpackFuelSlider.value = jetpackAbility.CurrentFuelRatio;
        // }
    }
    
    // Mise à jour de la barre de carburant du jetpack
    private void Update()
    {
        // On vérifie le carburant UNIQUEMENT si le jetpack est la capacité active
        // if (abilityManager.CanUseJetpack && jetpackFuelSlider != null)
        // {
        //     // Mise à jour continue de la barre de progression
        //     jetpackFuelSlider.value = jetpackAbility.CurrentFuelRatio;

        //     // PRO TIP: Feedback visuel rapide sur le faible niveau de carburant
        //     // if (jetpackAbility.CurrentFuelRatio < 0.2f)
        //     // {
        //     //     // Faire clignoter l'icône ou la couleur de la barre
        //     // }
        // }
    }
    
    // Cette méthode est appelée AUTOMATIQUEMENT lorsque l'événement se déclenche
    private void OnJumpLost()
    {
        // La perte du saut implique l'activation du jetpack (selon votre logique)
        UpdateHUD(false, true);

        // CONSEIL PRO : Vous pouvez ici jouer une animation ou un son de transition pour l'UI !
    }

    /// <summary>
    /// Met à jour l'affichage des icônes (Saut/Jetpack) en fonction des capacités disponibles.
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