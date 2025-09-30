using UnityEngine;

/// <summary>
/// Contient la logique pour la capacité de saut.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))] // S'assurer qu'un Rigidbody2D est présent
public class JumpAbility : MonoBehaviour
{
    [Header("Saut")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;

    private Rigidbody2D rb;
    private Animator animator; // La référence de l'Animator

    // Hashing des paramètres pour l'optimisation (meilleure pratique)
    public static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
    private bool wasGroundedLastFrame; // État de la frame précédente
    private bool isEnabled = true; // Démarre activé


    public void Initialize(Rigidbody2D rbRef, Animator animRef)
    {
        rb = rbRef;
        animator = animRef;
        wasGroundedLastFrame = IsGrounded();
    }

    void Awake()
    {
        // rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Vérifie si le joueur est au sol.
    /// </summary>
    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public void SetEnabled(bool state)
    {
        isEnabled = state;
    }

    /// <summary>
    /// Exécute la logique de saut.
    /// </summary>
    public void PerformJump()
    {
        // Ajout de la vérification de l'état de la capacité
        if (!isEnabled)
        {
            // Optionnel : Jouer un son ou un feedback visuel de capacité bloquée
            return;
        }

        if (IsGrounded())
        {
            // Appliquer la force de saut
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // Logique d'Animation : Déclenchement du saut
            if (animator != null)
            {
                animator.SetBool(IsJumpingHash, true);
            }
        }
    }

    // ------------------------
    // LOGIQUE ATTERISSAGE
    // ------------------------

    // Cette méthode doit être appelée par PlayerController.FixedUpdate()
    public void UpdateAnimationState()
    {
        if (animator == null) return;

        bool isCurrentlyGrounded = IsGrounded();

        // 1. Détecter l'atterrissage (vient de l'air et touche le sol)
        if (isCurrentlyGrounded && !wasGroundedLastFrame)
        {
            // L'atterrissage vient de se produire : réinitialiser l'état de saut
            animator.SetBool(IsJumpingHash, false);
            // Pro-Tip : Ici, vous pouvez déclencher un Trigger pour une animation d'Atterrissage (Landing)
            // animator.SetTrigger("Landing");
        }

        // 2. Détecter la chute (le saut n'a pas été exécuté, mais le joueur est en l'air)
        // Utile si le joueur marche depuis une plateforme.
        if (!isCurrentlyGrounded && rb.linearVelocity.y < -0.1f)
        {
            // Assurer que IsJumping est vrai pour les animations de chute/chute libre
            // Cela couvre les chutes des plateformes
            animator.SetBool(IsJumpingHash, true);
        }

        // Mettre à jour l'état pour la prochaine frame
        wasGroundedLastFrame = isCurrentlyGrounded;
    }
    
    public void HandleJumpInput()
    {
        if (!isEnabled) // isEnabled est l'état CanJump dans PlayerAbilityManager
        {
            // Jouer un son d'échec "boing" ou "ouïe"
            // Jouer une animation de petit "mini-hop" douloureux 
            return; 
        }
        
        // ... Logique de saut normale ...
    }
}