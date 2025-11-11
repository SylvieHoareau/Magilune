using UnityEngine;

/// <summary>
/// Gère la capacité de saut du joueur.
/// Fournit : Initialize(...), HandleJumpInput(bool), PerformJump(), ProcessJump(), IsGrounded(), SetEnabled(bool).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class JumpAbility : MonoBehaviour
{
    [Header("Paramètres de saut")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;

    // Références runtime
    private Rigidbody2D rb;
    private Animator animator;

    // États
    private bool wasGroundedLastFrame;
    private bool jumpRequested;

    [field: SerializeField]
    public bool IsEnabled { get; private set; } = true;

    // Hash d'animation optimisé
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");

    // -----------------------
    // Initialisation
    // -----------------------
    /// <summary>
    /// Doit être appelé par le PlayerController (ou par le manager) pour configurer rb/animator.
    /// </summary>
    public void Initialize(Rigidbody2D rbRef, Animator animRef)
    {
        rb = rbRef;
        animator = animRef;

        // Protection si groundCheck non assigné : utiliser la position du transform comme fallback
        if (groundCheck == null)
            groundCheck = this.transform;

        wasGroundedLastFrame = IsGrounded();
    }

    private void Reset()
    {
        // Valeurs par défaut pour faciliter le placement dans l'inspector
        groundCheckRadius = 0.12f;
    }

    // -----------------------
    // API publique
    // -----------------------

    /// <summary>
    /// Vérifie si le joueur touche le sol.
    /// </summary>
    public bool IsGrounded()
    {
        if (groundCheck == null)
            return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    /// <summary>
    /// Appelée par le PlayerInputHandler via le PlayerAbilityManager.
    /// Queue un saut si l'entrée est détectée (pratique pour traiter en FixedUpdate).
    /// </summary>
    public void HandleJumpInput(bool isPressed)
    {
        if (!IsEnabled) return;

        // On demande le saut seulement au moment de la pression (caller décide s'il passe true à chaque frame ou uniquement on-press)
        if (isPressed && IsGrounded())
            jumpRequested = true;
    }

    /// <summary>
    /// Exécute immédiatement un saut (vérifie IsEnabled & IsGrounded).
    /// Utile si le manager souhaite déclencher le saut dès l'input.
    /// </summary>
    public void PerformJump()
    {
        if (!IsEnabled || !IsGrounded() || rb == null) return;

        // Remplace la composante verticale de la vélocité
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        // Met l'animation de saut si dispo
        if (animator != null)
            animator.SetBool(IsJumpingHash, true);

        // on nettoie toute requête en attente
        jumpRequested = false;
    }

    /// <summary>
    /// A appeler depuis FixedUpdate() (ex: PlayerAbilityManager.FixedUpdate). Consomme jumpRequested si présent.
    /// </summary>
    public void ProcessJump()
    {
        if (jumpRequested)
        {
            PerformJump();
            jumpRequested = false;
        }

        UpdateAnimationState();
    }

    /// <summary>
    /// Permet d'activer/désactiver la capacité (utilisé par le manager pour power-ups / debugs)
    /// </summary>
    public void SetEnabled(bool state)
    {
        IsEnabled = state;
    }

    // -----------------------
    // Animation / état
    // -----------------------
    private void UpdateAnimationState()
    {
        if (animator == null || rb == null) return;

        bool isCurrentlyGrounded = IsGrounded();

        // Atterrissage
        if (isCurrentlyGrounded && !wasGroundedLastFrame)
        {
            animator.SetBool(IsJumpingHash, false);
        }

        // Si en l'air et descendant -> garde IsJumping true
        if (!isCurrentlyGrounded && rb.linearVelocity.y < -0.1f)
        {
            animator.SetBool(IsJumpingHash, true);
        }

        wasGroundedLastFrame = isCurrentlyGrounded;
    }

    // -----------------------
    // Debug visual
    // -----------------------
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
