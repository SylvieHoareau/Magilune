using UnityEngine;
using System;
using System.Collections; // Nécessaire pour le Coroutine

/// <summary>
/// Gère les capacités actuelles du joueur et leur état (activé/désactivé).
/// C'est le Cerveau Central qui contrôle quelles actions le joueur peut faire
/// </summary>
public class PlayerAbilityManager : MonoBehaviour
{
    [Header("Abilities State (Ce que le joueur peut faire)")]
    // État de la capacité de Saut. 'private set' empêche les autres scripts de le modifier directement.    
    [field: SerializeField] public bool CanJump { get; private set; } = true;
    // État de la capacité de Jetpack.
    [field: SerializeField] public bool CanUseJetpack { get; private set; } = true;
    // État de la capacité d'Escalade.
    [field: SerializeField] public bool CanClimb { get; private set; } = true;
    // État de la capacité de Grappin.
    [field: SerializeField] public bool CanGrapple { get; private set; } = true;

    [Header("Modules de Capacité (Les actions elles-mêmes)")]
    // Références directes aux scripts qui implémentent les actions (saut, jetpack, etc.)
    // Saut
    [SerializeField] private JumpAbility jumpAbility;
    // Jetpack
    [SerializeField] private JetpackAbility jetpackAbility;
    // Grapple
    [SerializeField] private GrappleAbility grappleAbility;
    // Climb
    [SerializeField] private ClimbAbility climbAbility;

    
    // Ces événements alertent l'HUD (ou d'autres systèmes) quand un état change.
    // L'Action<bool> envoie 'true' si activée, 'false' si désactivée.
    // Le booléen indique si la capacité est Active (true) ou Non (false)
    public event Action<bool> OnGrapplingActiveChanged;
    public event Action<bool> OnClimbingActiveChanged;
    public event Action<bool> OnJetpackActiveChanged;

    // Evénement spéial pour signaler la perte de capacité de Saut
    public event Action OnJumpCapabilityLost;

    
    private void Start()
    {
        // Pour des tests rapides ou des triggers scénarisés
        // LoseJumpCapability(); 
        if (jetpackAbility == null)
        {
            jetpackAbility = GetComponent<JetpackAbility>();
            if (jetpackAbility == null)
            {
                Debug.LogWarning("PlayerAbilityManager : Référence JetPackAbility manquante.");
            }
        }

        // Vérification du Grappling
        if (grappleAbility == null)
        {
            grappleAbility = GetComponent<GrappleAbility>();
            if (grappleAbility == null)
            {
                Debug.LogWarning("PlayerAbilityManager : Référence JetPackAbility manquante.");
            }
        }

        // Vérification du climbing
        if (climbAbility == null)
        {
            climbAbility = GetComponent<ClimbAbility>();
            if (climbAbility == null)
            {
                Debug.LogWarning("PlayerAbilityManager : Référence ClimbAbility manquante.");
            }
        }

        // Assurer l'état initial des capacités (si non fait dans l'Inspector)
        CanClimb = climbAbility != null; // Si le module est là, la capacité est là.
        CanGrapple = grappleAbility != null;
    }

    /// <summary>
    /// Logique pour la perte de la capacité de saut (due au trauma à la jambe).
    /// </summary>
    public void LoseJumpCapability()
    {
        if (CanJump)
        {
            CanJump = false;

            // Désactiver la capacité dans JumpAbility
            if (jumpAbility != null)
            {
                jumpAbility.SetEnabled(false);
                // jumpAbility.IsEnabled = false;
            }
            Debug.Log("Capacité de Saut perdue : Trauma à la jambe !");

            // Déclenche l'événement pour la caméra et le feedback visuel/sonore
            OnJumpCapabilityLost?.Invoke();

            // Activer le grappling
            grappleAbility?.SetEnabled(true);
            Debug.Log("Capacité de grappling activée !");

            // Activer l'escalade
            climbAbility?.SetEnabled(true);
            Debug.Log("Capacité d'escalade activée !");

            // Active le JetPack comme alternative
            EnableJetpackCapability();
        }
    }

    // Méthode pour un retour de capacité (si nécessaire)
    public void RegainJumpCapability()
    {
        CanJump = true;
        CanUseJetpack = false;
        Debug.Log("Capacité de Saut retrouvée !");
    }

    /// <summary>
    /// Active la capacité de JetPack.
    /// </summary>
    private void EnableJetpackCapability()
    {
        if (jetpackAbility != null)
        {
            CanUseJetpack = true;
            // Activer le composant JetpackAbility s'il était désactivé au démarrage
            jetpackAbility.enabled = true; 
            Debug.Log("JetPack activé comme alternative au saut !");
        }
        else
        {
            Debug.LogWarning("PlayerAbilityManager : Impossible d'activer le JetPack, référence manquante.");
        }
    }

    /// <summary>
    /// Transmet la commande du joueur au JetPackAbility
    /// </summary>
    public void HandleJetpackInput(bool isPressed)
    {
        if (CanUseJetpack && jetpackAbility != null)
        {
            jetpackAbility.HandleJetPack(isPressed);
        }
    }

    /// <summary>
    /// Transmet la commande du joueur au GrappleAbility
    /// </summary>
    public void HandleGrappleInput(bool isPressed)
    {
        if (grappleAbility != null)
        {
            grappleAbility.HandleGrappleInput(isPressed);
        }
    }

    /// <summary>
    /// Transmet la commande de mouvement vertical au ClimbAbility
    /// </summary>
    public void HandleClimbInput(Vector2 moveInput)
    {
        if (CanClimb && climbAbility != null)
        {
            climbAbility.HandleClimbInput(moveInput);
        }
    }

    // --------------------------------------------------------------------------
    // FONCTIONS DE CONTROLE (Appelées par le PlayerController ou le Debug)
    // --------------------------------------------------------------------------

    /// <summary>
    /// Définit l'état de la capacité de Saut.
    /// </summary>
    /// <param name="state">Le nouvel état (true=actif, false=inactif).</param>
    public void SetJumpCapability(bool state)
    {
        CanJump = state;
        Debug.Log($"Capacité de Saut réglée à : {state}");

        // Dire au module JumpAbility s'il peut fonctionner
        if (jumpAbility != null)
        {
            jumpAbility.SetEnabled(state);
        }

        Debug.Log($"Capacité de Saut réglée à : {state}");
        
        // Si le saut est désactivé, on alerte pour la transition (ex: l'activation du Jetpack)
        if (!state)
        {
            OnJumpCapabilityLost?.Invoke();
        }
    }

    /// <summary>
    /// Définit l'état de la capacité de Jetpack. Utilisé pour le Debug.
    /// </summary>
    public void SetJetpackCapability(bool state)
    {
        CanUseJetpack = state;
        Debug.Log($"Capacité de Jetpack réglée à : {state}");

        // La logique ici est pour le débogage. Si le jetpack a un état Enable/Disable.
        if (jetpackAbility != null)
        {
            // On peut ajouter une fonction SetEnabled() au JetpackAbility si nécessaire
        }
        
        // Alerter l'HUD : "L'état du Jetpack a changé!"
        OnJetpackActiveChanged?.Invoke(state); 
        Debug.Log($"Capacité de Jetpack réglée à : {state}");
    }

    /// <summary>
    /// Définit l'état de la capacité de Grappling. Utilisé pour le Debug.
    /// </summary>
    public void SetGrappleCapability(bool state)
    {
        CanGrapple = state;

        if (grappleAbility != null)
        {
            // Demander au module de grappin de s'activer/désactiver
            grappleAbility.SetEnabled(state);
        }
        
        // Alerter l'HUD : "L'état du Grappin a changé!"
        OnGrapplingActiveChanged?.Invoke(state); 
        Debug.Log($"Capacité de Grappling réglée à : {state}");
    }

    /// <summary>
    /// Définit l'état de la capacité de Climb (Escalade). Utilisé pour le Debug.
    /// </summary>
    public void SetClimbCapability(bool state)
    {
        CanClimb = state;

        if (climbAbility != null)
        {
            // Demander au module d'escalade de s'activer/désactiver
            climbAbility.SetEnabled(state);
        }

        // Alerter l'HUD : "L'état de l'Escalade a changé!"
        OnClimbingActiveChanged?.Invoke(state); 
        Debug.Log($"Capacité d'Escalade réglée à : {state}");
    }

}