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
    
    // Le gestionnaire d'événements du joueur
    [SerializeField] private PlayerAbilityManager abilityManager;

    private void Start()
    {
        // Récupérer la référence au JetpackAbility
        if (abilityManager != null)
        {
            Debug.LogError("AbilityHUD : Référence PlayerAbilityManager manquante. Assignez-la !");
            return;
        }

        // Abonnement à l'événement de perte de capacité (le moment de la transition)
        abilityManager.OnJumpCapabilityLost += OnJumpLost;

        // NOUVEAUX ABONNEMENTS pour les activations/désactivations (Grappin, Jetpack, Grimpe)
        abilityManager.OnGrapplingActiveChanged += UpdateGrapplingIcon;
        abilityManager.OnClimbingActiveChanged += UpdateClimbingIcon;
        // Si le Jetpack est aussi géré par état :
        // abilityManager.OnJetpackActiveChanged += UpdateJetpackIcon;


        // INITIALISATION DE TOUS LES ÉTATS (On cache tout au début si les capacités sont inactives)
        // Ceci est une étape critique pour éviter que les icônes ne s'affichent au démarrage
        if (jumpIconContainer != null)
        {
            jumpIconContainer.SetActive(false);
        }
        if (grapplingIconContainer != null)
        {
            grapplingIconContainer.SetActive(false);
        }
        if (climbingIconContainer != null)
        {
            climbingIconContainer.SetActive(false);
        }
        if (jetpackIconContainer != null)
        {
            jetpackIconContainer.SetActive(false);
        }

        // Initialisation de l'état au démarrage du jeu
        UpdateHUD(abilityManager.CanJump, abilityManager.CanUseJetpack);
        
    }
    
    private void Update()
    {
       
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

    /// <summary>
    /// Gère l'affichage de l'icône de grappin.
    /// Appelé via l'événement OnGrapplingActiveChanged.
    /// </summary>
    /// <param name="isActive">Vrai si le joueur est en train de se grappiner.</param>
    private void UpdateGrapplingIcon(bool isActive)
    {
        if (grapplingIconContainer != null)
        {
            grapplingIconContainer.SetActive(isActive);
            // Jouer un son
        }
    }

    /// <summary>
    /// Gère l'affichage de l'icône de grimpe.
    /// Appelé via l'événement OnClimbingActiveChanged.
    /// </summary>
    private void UpdateClimbingIcon(bool isActive)
    {
        if (climbingIconContainer != null)
        {
            climbingIconContainer.SetActive(isActive);
        }
    }
    
    /// <summary>
    /// Gère l'affichage de l'icône de jetpack.
    /// Appelé via l'événement OnClimbingActiveChanged.
    /// </summary>
    private void UpdateJetpackIcon(bool isActive)
    {
        if (jetpackIconContainer != null)
        {
            jetpackIconContainer.SetActive(isActive);
        }
    }

    /// <summary>
    /// Ancienne logique (si nécessaire), mise à jour pour utiliser la nouvelle fonction
    /// </summary>
    private void OnJumpLost()
    {
        // Votre ancienne logique de transition (Saut -> Jetpack)
        if (jumpIconContainer != null)
        {
            jumpIconContainer.SetActive(false);
        }
        if (jetpackIconContainer != null)
        {
            jetpackIconContainer.SetActive(true);
        }
    }

    // Nettoyage
    private void OnDestroy()
    {
        // DÉSABONNEMENT POUR ÉVITER LES FUITES DE MÉMOIRE !
        if (abilityManager != null)
        {
            abilityManager.OnJumpCapabilityLost -= OnJumpLost;
            abilityManager.OnGrapplingActiveChanged -= UpdateGrapplingIcon;
            abilityManager.OnClimbingActiveChanged -= UpdateClimbingIcon;
            abilityManager.OnJetpackActiveChanged -= UpdateJetpackIcon;
        }
    }
}