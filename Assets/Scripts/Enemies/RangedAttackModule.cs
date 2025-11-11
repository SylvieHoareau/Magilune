// RangedAttackModule.cs
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Gère la logique d'attaque par projectile pour les ennemis à distance.
/// </summary>
public class RangedAttackModule : MonoBehaviour
{
    [Header("Ranged Attack")]
    // DEFINITION DE ATTACKRANGE DANS EnemyCore POUR LA LOGIQUE GLOBALE
    // Permet de lire la portée dans EnemyCore et de l'éditer dans l'inspecteur
    [field: SerializeField] public float AttackRange { get; private set; } = 5f;    

    [Tooltip("Prefab du projectile (StarProjectile, Bullet, etc.)")]
    [SerializeField] private GameObject projectilePrefab; // Utilisation de StarProjectile, comme dans votre PlayerAttack
    public Transform firePoint;            // Point de spawn du projectile
    [SerializeField] private float projectileSpeed = 10f;    // Vitesse de base si non définie par le projectile lui-même

    // Définition du Cooldown (pour TryShoot dans EnemyCore)
    [Header("Cooldown")]
    [SerializeField] private float shootCooldown = 2f;      // Temps entre chaque tir
    private float lastShootTime; // Temps du dernier tir
    private Animator _animator;

    private bool isActive = false;


    private void Awake()
    {
        _animator = GetComponent<Animator>();

        // Initialiser le temps du dernier tir pour permettre un tir immédiat au début
        lastShootTime = -shootCooldown; // Permet de tirer immédiatement au début
    }

    public void SetModuleActive(bool state)
    {
        isActive = state;
    }

    // ----- Logique de Tir (appelée par Enemy / Player)

    // DEFINITIONS DE TRYSHOOT (appelé par EnemyCore)
    /// <summary>
    /// Tente de tirer un projectile si le cooldown est écoulé.
    /// </summary>
    /// <returns>True si le tir a été effectué, false sinon.</returns>
    public bool TryShoot(Vector2 direction)
    {
        if (Time.time >= lastShootTime + shootCooldown)
        {
            // Le cooldown est écoulé, on peut tirer
            Shoot(direction);
            lastShootTime = Time.time; // Met à jour le temps du dernier tir
            // Jouer l'animation de tir si disponible
            if (_animator != null)
            {
                _animator.SetTrigger("Shoot");
            }
            return true;
        }
        return false; // Cooldown pas encore écoulé
    }

    /// <summary>
    /// Effectue l'action de tir du projectile dans la direction spécifiée.
    /// </summary>
    /// <param name="shootDirection">La direction normalisée du tir.</param>

    public void Shoot(Vector2 shootDirection)
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError($"RangedAttackModule sur {gameObject.name} n'a pas de prefab de projectile assigné.");
            return;
        }

        // Déclencher l'animation de tir si l'ennemi en a une
        if (_animator != null)
        {
             // Assurez-vous que l'Animator de l'ennemi a un paramètre Trigger appelé "Shoot"
            _animator.SetTrigger("Shoot"); 
        }

        // Instancier le projectile au point de tir
        GameObject newProjectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Appliquer la vélocité au Rigidbody2D du projectile
        Rigidbody2D bulletRb = newProjectileGO.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            // Appliquer la vitesse au projectile
            bulletRb.linearVelocity = shootDirection * projectileSpeed;

            // Appliquer également une rotation de la vélocité pour le visuel
            // S'assurer que l'axe Y positif de la balle est vers le haut (sa direction)
            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
            newProjectileGO.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        else
        {
            Debug.LogError($"Le Prefab de projectile sur {gameObject.name} n'a pas de Rigidbody2D pour appliquer la vitesse !");
            Destroy(newProjectileGO); // Nettoyer l'instance si mal configurée
        }

        if (firePoint == null)
        {
            Debug.LogError($"RangedAttackModule sur {gameObject.name} n'a pas de Fire Point assigné.");
            return;
        }
    }
}