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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    /// <summary>
    /// Vérifie si le joueur est au sol.
    /// </summary>
    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    /// <summary>
    /// Exécute la logique de saut.
    /// </summary>
    public void PerformJump()
    {
        if (IsGrounded())
        {
            // Appliquer la force de saut
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            // Optionnel: Mettre à jour l'Animator.SetBool("IsJumping", true);
        }
    }
}