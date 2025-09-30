// RangedAttackModule.cs
using UnityEngine;

/// <summary>
/// Gère la logique d'attaque par projectile pour les ennemis à distance.
/// </summary>
public class RangedAttackModule : MonoBehaviour
{
    [Header("Ranged Attack")]
    [Tooltip("Prefab du projectile (StarProjectile, Bullet, etc.)")]
    [SerializeField] private GameObject projectilePrefab; // Utilisation de StarProjectile, comme dans votre PlayerAttack
    [SerializeField] private Transform firePoint;            // Point de spawn du projectile
    [SerializeField] private float projectileSpeed = 10f;    // Vitesse de base si non définie par le projectile lui-même
    
    // Référence au SpriteRenderer pour déterminer la direction du tir
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        // Récupérer le SpriteRenderer pour vérifier la direction d'orientation (flipX ou scale.x)
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // Tentative de récupération dans le parent si non trouvé
        if (_spriteRenderer == null)
        {
            // Peut être sur un enfant ou un parent, mais pour un ennemi simple, il est souvent sur le root
            _spriteRenderer = GetComponentInParent<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Effectue l'action de tir du projectile.
    /// </summary>
    public void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError($"RangedAttackModule sur {gameObject.name} n'a pas de prefab de projectile assigné.");
            return;
        }

        // 1. Instancier le projectile au point de tir
        GameObject newProjectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        // 2. Déterminer la direction de tir
        float directionX = 1f; // Par défaut : droite

        // float directionX = (_spriteRenderer != null && _spriteRenderer.flipX) ? -1f : transform.localScale.x;
        if (_spriteRenderer != null)
        {
            // Fallback si pas de SR, on utilise le scale.x (si l'ennemi se retourne via scale)
            directionX = _spriteRenderer.flipX ? -1f : 1f;
        }
        else
        {
            // Fallback: Si pas de SpriteRenderer, on utilise la scale X du Transform
            directionX = transform.localScale.x > 0 ? 1f : -1f;
        }

        Vector2 shootDirection = new Vector2(directionX, 0f);

        // 3. Appliquer la vélocité au Rigidbody2D du projectile
        Rigidbody2D bulletRb = newProjectile.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = shootDirection * projectileSpeed;
        }
        else
        {
            Debug.LogError($"Le Prefab de projectile sur {gameObject.name} n'a pas de Rigidbody2D pour appliquer la vitesse !");
        }

        if (firePoint == null)
        {
            Debug.LogError($"RangedAttackModule sur {gameObject.name} n'a pas de Fire Point assigné.");
            return;
        }
    }
}