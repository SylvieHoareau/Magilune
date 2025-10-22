using UnityEngine;

public class ClimbAbility : MonoBehaviour
{
    // --- Références ---
    private Rigidbody2D rb;
    private Animator animator;
    
    // --- Paramètres d'Escalade ---
    [Header("Climb Settings")]
    [SerializeField] private float climbSpeed = 5f; // Vitesse d'escalade verticale
    [SerializeField] private LayerMask climbableLayer; // Couche des murs/surfaces à escalader
    [SerializeField] private Transform wallCheck; // Point de détection du mur (sur le côté du joueur)
    [SerializeField] private float wallCheckDistance = 0.2f; // Distance de détection
    
    // --- État Interne ---
    private bool isClimbing = false;
    private bool isClimbActive = true; // L'état d'activité pour l'AbilityManager

    // Hashing des paramètres pour l'optimisation
    public static readonly int IsClimbingHash = Animator.StringToHash("IsClimbing");

    // --- Initialisation ---
    public void Initialize(Rigidbody2D rbRef, Animator animRef)
    {
        rb = rbRef;
        animator = animRef;
    }

    // --- Détection du Mur (Appelée par PlayerController) ---
    public bool IsTouchingClimbableWall()
    {
        // Utilise un Raycast pour vérifier si un mur est détecté par le WallCheck
        // La direction est basée sur la rotation du joueur (transform.localScale.x)
        float direction = transform.localScale.x;
        
        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position, 
            Vector2.right * direction, 
            wallCheckDistance, 
            climbableLayer
        );

        // Visualisation dans la scène (utile pour le debug)
        Color debugColor = hit.collider != null ? Color.green : Color.red;
        Debug.DrawRay(wallCheck.position, Vector2.right * direction * wallCheckDistance, debugColor);

        return hit.collider != null;
    }

    // --- Gérer l'Entrée du Joueur (Appelée par PlayerAbilityManager) ---
    public void HandleClimbInput(Vector2 moveInput)
    {
        if (!isClimbActive || rb == null) return;

        bool touchingWall = IsTouchingClimbableWall();
        float verticalInput = moveInput.y;

        // Démarrer l'escalade
        if (touchingWall && Mathf.Abs(verticalInput) > 0.1f)
        {
            StartClimbing(verticalInput);
        }
        // Continuer l'escalade
        else if (isClimbing)
        {
            // Arrêter l'escalade si le joueur n'a plus d'input vertical
            if (Mathf.Abs(verticalInput) < 0.1f)
            {
                 StopClimbing(true); // Arrêter tout en restant "collé" (vitesse nulle)
            }
            // Mettre à jour la vitesse
            else
            {
                 rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalInput * climbSpeed);
            }
        }
        // 3. Arrêter l'escalade si le mur est perdu et qu'on était en train de grimper
        else if (!touchingWall && isClimbing)
        {
             StopClimbing(false); // Arrêter et rendre la physique
        }
    }
    
    private void StartClimbing(float verticalInput)
    {
        if (isClimbing) return;

        isClimbing = true;
        // Désactiver ou réduire fortement la gravité pour l'escalade
        rb.gravityScale = 0f; 
        
        // Arrêter la vitesse horizontale (pour ne pas glisser)
        rb.linearVelocity = Vector2.zero; 
        
        // Appliquer la vitesse verticale immédiatement
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalInput * climbSpeed);

        // Mettre à jour l'animation
        animator.SetBool(IsClimbingHash, true);
    }

    public void StopClimbing(bool stickToWall)
    {
        if (!isClimbing) return;
        
        isClimbing = false;
        
        // Rétablir la gravité normale
        rb.gravityScale = 3f; 
        
        // Si l'on veut rester coller (pour un mur d'adhérence)
        if (stickToWall)
        {
             rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Vitesse verticale nulle
             rb.gravityScale = 0f; // Garder la gravité à 0
        }
        
        // Mettre à jour l'animation
        animator.SetBool(IsClimbingHash, false);
    }
    
    public bool IsClimbing() => isClimbing;
    
    // --- Gestion de l'état externe (PlayerAbilityManager) ---
    public void SetEnabled(bool state)
    {
        isClimbActive = state;
        if (!state && isClimbing)
        {
            StopClimbing(false);
        }
    }
    
    // Pour visualiser la zone de détection dans l'éditeur (Gizmos)
    private void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.yellow;
            float direction = transform.localScale.x;
            Vector3 targetPosition = wallCheck.position + (Vector3)Vector2.right * direction * wallCheckDistance;
            Gizmos.DrawLine(wallCheck.position, targetPosition);
            Gizmos.DrawWireSphere(targetPosition, 0.05f);
        }
    }
}