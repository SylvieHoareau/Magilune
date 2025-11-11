using UnityEngine;

/// <summary>
/// Gère et centralise les capacités du joueur : saut, jetpack, grappin, escalade.
/// Sert d’interface entre le PlayerController et les scripts de capacités.
/// </summary>
public class PlayerAbilityManager : MonoBehaviour
{
    [Header("Capacités du joueur")]
    [SerializeField] private JumpAbility jumpAbility;
    [SerializeField] private JetpackAbility jetpackAbility;
    [SerializeField] private GrappleAbility grappleAbility;
    [SerializeField] private ClimbAbility climbAbility;

    // --- États d'activation ---
    public bool CanJump { get; private set; } = true;
    public bool CanUseJetpack { get; private set; } = true;
    public bool CanClimb { get; private set; } = true;
    public bool CanGrapple { get; private set; } = true;

    private void Awake()
    {
        // Auto-récupération des références manquantes
        if (jumpAbility == null)
            jumpAbility = GetComponentInChildren<JumpAbility>();

        if (jetpackAbility == null)
            jetpackAbility = GetComponentInChildren<JetpackAbility>();

        if (grappleAbility == null)
            grappleAbility = GetComponentInChildren<GrappleAbility>();

        if (climbAbility == null)
            climbAbility = GetComponentInChildren<ClimbAbility>();
    }

    // --- Gestion des entrées ---

    /// <summary>
    /// Gère le saut (press/tap)
    /// </summary>
    public void HandleJumpInput(bool jumpPressed)
    {
        if (!CanJump || jumpAbility == null)
            return;

        if (jumpPressed)
            jumpAbility.PerformJump();
    }

    /// <summary>
    /// Active ou désactive le jetpack selon si le joueur maintient la touche.
    /// </summary>
    public void HandleJetpackInput(bool isPressed)
    {
        if (!CanUseJetpack || jetpackAbility == null)
            return;

        jetpackAbility.HandleJetPack(isPressed);
    }

    /// <summary>
    /// Gère le grappin : démarrage et arrêt.
    /// </summary>
    public void HandleGrappleInput(bool isPressed)
    {
        if (!CanGrapple || grappleAbility == null)
            return;

        if (isPressed)
            grappleAbility.StartGrapple();
        else
            grappleAbility.StopGrapple();
    }

    /// <summary>
    /// Gère l'entrée de mouvement pour l'escalade.
    /// </summary>
    public void HandleClimbInput(Vector2 moveInput)
    {
        if (!CanClimb || climbAbility == null)
            return;

        climbAbility.HandleClimbInput(moveInput);
    }

    // --- Activation / désactivation des capacités (debug ou power-up) ---
    public void SetJumpCapability(bool enabled)
    {
        CanJump = enabled;
        Debug.Log($"Saut {(enabled ? "activé" : "désactivé")}");
    }

    public void SetJetpackCapability(bool enabled)
    {
        CanUseJetpack = enabled;
        Debug.Log($"Jetpack {(enabled ? "activé" : "désactivé")}");
    }

    public void SetClimbCapability(bool enabled)
    {
        CanClimb = enabled;
        Debug.Log($"Escalade {(enabled ? "activée" : "désactivée")}");
    }

    public void SetGrappleCapability(bool enabled)
    {
        CanGrapple = enabled;
        Debug.Log($"Grappin {(enabled ? "activé" : "désactivé")}");
    }

    // --- Vérifie si le joueur utilise actuellement une capacité ---
    public bool IsUsingAnyAbility()
    {
        return (jetpackAbility != null && jetpackAbility.IsUsingJetpack)
            || (grappleAbility != null && grappleAbility.IsGrappling())
            || (climbAbility != null && climbAbility.IsClimbing());
    }
}
