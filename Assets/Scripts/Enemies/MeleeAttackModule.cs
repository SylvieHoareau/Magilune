using UnityEngine;

/// <summary>
/// Seul but : gérer l'attaque de proximité
/// </summary>
public class MeleeAttackModule : MonoBehaviour
{
    [Header("Stats d'Attaque")]
    [SerializeField] private float damage = 1f; // Dégâts à appliquer
    [field: SerializeField] public float AttackRange { get; private set; } = 1.0f;
    [SerializeField] private float attackCooldown = 1.0f;

    [Header("Hitbox")]
    [SerializeField] private float hitBoxRadius = 0.5f; // Rayon de la zone de coup
    [SerializeField] private LayerMask playerLayer; // Layer sur lequel se trouve le joueur

    private float lastAttackTime;
    private Animator animator;

    private bool isActive = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetModuleActive(bool state)
    {
        isActive = state;
    }

    public bool TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // 1. Déclencher l'animation d'attaque
            animator.SetTrigger("Attack");

            lastAttackTime = Time.time;
            return true;
        }
        return false;
    }

    // Fonction appelée par un Animation Event au point d'impact de l'animation
    public void ApplyDamageHitbox()
    {
        // Déterminer le point central de la Hitbox
        // L'attaque se fait généralement DEVANT l'ennemi, pas sur son centre.
        // On utilise transform.localScale.x pour savoir s'il regarde à droite (1) ou à gauche (-1).
        float direction = transform.localScale.x > 0 ? 1f : -1f;
        Vector2 hitPoint = (Vector2)transform.position + new Vector2(direction * hitBoxRadius, 0);

        // Détection instantanée des cibles dans la zone de frappe
        // Physics2D.OverlapCircleAll vérifie tous les colliders dans un cercle (2D) donné.
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(hitPoint, hitBoxRadius, playerLayer);

        // Traiter les cibles (souvent une seule, le joueur)
        foreach (Collider2D target in hitTargets)
        {
            // Utilisation du tag pour confirmer que c'est le joueur
            if (target.CompareTag("Player"))
            {
                // Tenter de récupérer le composant PlayerHealth sur le GameObject touché
                PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();

                if (playerHealth != null)
                {
                    // **APPLIQUER LES DÉGÂTS**
                    playerHealth.TakeDamage(damage);
                    Debug.Log($"Melee hit : Dégâts appliqués au joueur ({damage}).");
                    
                    // Pro-Tip : On peut mettre ici un break si on ne veut frapper qu'une seule cible à la fois
                    // break; 
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Dessine la zone de frappe dans l'éditeur (visible uniquement quand l'objet est sélectionné)
        float direction = transform.localScale.x > 0 ? 1f : -1f;
        Vector2 hitPoint = (Vector2)transform.position + new Vector2(direction * hitBoxRadius, 0);
        
        Gizmos.color = Color.red;
        // Utiliser DrawWireSphere pour voir le cercle en fil de fer
        Gizmos.DrawWireSphere(hitPoint, hitBoxRadius);
    }
}