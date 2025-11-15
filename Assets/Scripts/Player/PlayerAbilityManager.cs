using System;
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

    // --- Evenements envoyés au HUD ---
    public event Action OnJumpCapabilityLost;
    public event Action<bool> OnGrapplingActiveChanged;
    public event Action<bool> OnClimbingActiveChanged;
    public event Action<bool> OnJetpackActiveChanged;

    // --- Etats internes ---
    private bool _canJump = true;
    private bool _canUseJetpack = true;
    private bool _canClimb = true;
    private bool _canGrapple = true;

    public bool CanJump => _canJump;
    public bool CanUseJetpack => _canUseJetpack;
    public bool CanClimb => _canClimb;
    public bool CanGrapple => _canGrapple;

    // --- États d'activation ---
    // public bool CanJump { get; private set; } = true;
    // public bool CanUseJetpack { get; private set; } = true;
    // public bool CanClimb { get; private set; } = true;
    // public bool CanGrapple { get; private set; } = true;

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
        if (!_canJump || jumpAbility == null)
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

        bool wasUsing = jetpackAbility.IsUsingJetpack;

        jetpackAbility.HandleJetPack(isPressed);

        // Déclenche l'événement uniquement si l'état change

    }

    /// <summary>
    /// Gère le grappin : démarrage et arrêt.
    /// </summary>
    public void HandleGrappleInput(bool isPressed)
    {
        if (!_canGrapple || grappleAbility == null)
            return;

        bool wasActive = grappleAbility.IsGrappling();

        if (isPressed)
            grappleAbility.StartGrapple();
        else
            grappleAbility.StopGrapple();

        if (grappleAbility.IsGrappling() != wasActive)
            OnGrapplingActiveChanged?.Invoke(grappleAbility.IsGrappling());
    }

    /// <summary>
    /// Gère l'entrée de mouvement pour l'escalade.
    /// </summary>
    public void HandleClimbInput(Vector2 moveInput)
    {
        if (!_canClimb || climbAbility == null)
            return;

        bool wasClimbing = climbAbility.IsClimbing();

        climbAbility.HandleClimbInput(moveInput);

        if (climbAbility.IsClimbing() != wasClimbing)
            OnClimbingActiveChanged?.Invoke(climbAbility.IsClimbing());

    }

    /// <summary>
    /// Peret d'informer manuellement le HUD ou d'autres systèmes
    /// que l'état du grappin a changé (par exemple après une action spéciale)
    /// </summary>
    /// <param name="enabled"></param>
    public void NotifyGrappleActionState(bool isActive)
    {
        OnGrapplingActiveChanged?.Invoke(isActive);
    }

    /// <summary>
    /// Désactive la capacité de saut et notifie le HUD
    /// Utilisé lorsqu'un événement de gameplay retire le saut au joueur
    /// </summary>
    /// <param name="enabled"></param>
    public void LoseJumpCapability()
    {
        if (!_canJump) return;

        _canJump = false;

        if (jumpAbility != null)
            jumpAbility.SetEnabled(false);

        // Notifier l'interface : bascule du saut vers jetpack
        OnJumpCapabilityLost?.Invoke();

        Debug.Log("Capacité de saut PERDUE !");
    }

    // --- Activation / désactivation des capacités (debug ou power-up) ---
    public void SetJumpCapability(bool enabled)
    {
        bool wasEnabled = _canJump;
        _canJump = enabled;

        if (wasEnabled && !enabled)
            OnJumpCapabilityLost?.Invoke();

        Debug.Log($"Saut {(enabled ? "activé" : "désactivé")}");
    }

    public void SetJetpackCapability(bool enabled)
    {
        _canUseJetpack = enabled;
        Debug.Log($"Jetpack {(enabled ? "activé" : "désactivé")}");
    }

    public void SetClimbCapability(bool enabled)
    {
        _canClimb = enabled;
        Debug.Log($"Escalade {(enabled ? "activée" : "désactivée")}");
    }

    public void SetGrappleCapability(bool enabled)
    {
        _canGrapple = enabled;
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
