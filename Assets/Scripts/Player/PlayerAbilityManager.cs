using UnityEngine;
using System.Collections; // Nécessaire pour le Coroutine

/// <summary>
/// Gère les capacités actuelles du joueur et leur état (activé/désactivé).
/// </summary>
public class PlayerAbilityManager : MonoBehaviour
{
    [Header("Abilities State")]
    // Rendre l'état public pour que d'autres scripts puissent le lire (ex: PlayerController)
    [field: SerializeField] public bool CanJump { get; private set; } = true;
    [field: SerializeField] public bool CanUseJetpack { get; private set; } = true;
    [field: SerializeField] public bool CanClimb { get; private set; } = true;
    [field: SerializeField] public bool CanGrapple { get; private set; } = true;

    [Header("Modules reliés")]
    [SerializeField] private JumpAbility jumpAbility;
    // Jetpack
    [SerializeField] private JetpackAbility jetpackAbility;
    // Grapple
    [SerializeField] private GrappleAbility grappleAbility; 
    // Climb
    [SerializeField] private ClimbAbility climbAbility;
    // Événement pour signaler la perte de capacité au reste du système (Caméra, UI, etc.)
    public event System.Action OnJumpCapabilityLost;

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

    /// <summary>
    /// Définit l'état de la capacité de Saut. Utilisé pour le Debug.
    /// </summary>
    public void SetJumpCapability(bool state)
    {
        CanJump = state;
        Debug.Log($"Capacité de Saut réglée à : {state}");
        
        // Si vous désactivez le saut, vous pourriez vouloir désactiver également l'implémentation
        if (jumpAbility != null)
        {
            jumpAbility.SetEnabled(state);
        }
    }

    /// <summary>
    /// Définit l'état de la capacité de Jetpack. Utilisé pour le Debug.
    /// </summary>
    public void SetJetpackCapability(bool state)
    {
        CanUseJetpack = state;
        Debug.Log($"Capacité de Jetpack réglée à : {state}");
    }

    /// <summary>
    /// Définit l'état de la capacité de Grappling. Utilisé pour le Debug.
    /// </summary>
    public void SetGrappleCapability(bool state)
    {
        CanGrapple = state;
        Debug.Log($"Capacité de Grappling réglée à : {state}");
        
        if (grappleAbility != null)
        {
            // La logique exacte dépend de votre module, mais généralement on désactive/active l'implémentation
            grappleAbility.SetEnabled(state); 
        }
    }

    /// <summary>
    /// Définit l'état de la capacité de Climb (Escalade). Utilisé pour le Debug.
    /// </summary>
    public void SetClimbCapability(bool state)
    {
        CanClimb = state;
        Debug.Log($"Capacité d'Escalade réglée à : {state}");

        if (climbAbility != null)
        {
            // Similaire au Grappling, on contrôle l'état du module si implémenté
            climbAbility.SetEnabled(state);
        }
    }

}