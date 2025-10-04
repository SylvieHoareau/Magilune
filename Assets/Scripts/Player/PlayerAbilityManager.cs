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

    [Header("Modules reliés")]
    [SerializeField] private JumpAbility jumpAbility;
    [SerializeField] private JetpackAbility jetpackAbility;
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
                // Pro-tip: Vous pouvez désactiver le composant JumpAbility si vous êtes sûr de ne plus l'utiliser.
                // jumpAbility.enabled = false;
            }
            Debug.Log("Capacité de Saut perdue : Trauma à la jambe !");

            // Déclenche l'événement pour la caméra et le feedback visuel/sonore
            OnJumpCapabilityLost?.Invoke();

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
}