using UnityEngine;

/// <summary>
/// Gère le mouvement, la collision et la durée de vie d'un projectile ennemi
/// </summary>
/// 
public class EnemyBullet : MonoBehaviour
{
    [Header("Mouvement")]
    [Tooltip("Vitesse de déplacement de la balle")]
    [SerializeField] private float speed = 10f;

    [Tooltip("Dégâts infligés au joueur.")]
    [SerializeField] private int damageAmount = 1;

    [Header("Durée de Vie")]
    [Tooltip("Temps maximal avant que la balle ne soit détruite")]
    [SerializeField] private float lifetime = 3f;

    // Vecteur de direction, doit être défini par le Gunnner lors de l'instance
    private Vector2 moveDirection;
    private Rigidbody2D rb2D;

    // ------------------------------------
    // LIFECYCLE
    // ------------------------------------

    void Awake()
    {
        // On récupère le Rigidbody2D si l'on veut gérer le mouvement via la physique
        // C'est souvent plus précis pour les collisions rapides
        rb2D = GetComponent<Rigidbody2D>();

        // Vérifier si on a un Rigidbody2D sur le prefab de balle !
        if (rb2D == null)
        {
            Debug.LogError("Rigidbody2D manquant sur le prefab de balle !");
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Démarre un compte à rebours pour la destruction
        Destroy(gameObject, lifetime);

        // On applique la force initiale ou la vélocité
        // Utiliser la vélocité pour un mouvement linéaire
        if (rb2D != null)
        {
            rb2D.linearVelocity = moveDirection * speed;
        }
    }

    // -----------------------------------
    // Méthodes d'initialisation
    // -----------------------------------

    /// <summary>
    /// Définit la direction de la balle. Doit être appelé par le Gunner au moment du tir.
    /// </summary>
    /// <param name="direction">Le vecteur de direction (normalisé) du tir.</param>
    public void SetDirection(Vector2 direction)
    {
        // Normaliser assure que le vecteur est de longueur 1,
        // ce qui garantit une vitesse uniforme (multipliée par 'speed').
        moveDirection = direction.normalized;

        // Optionnel: Rotation du sprite pour qu'il pointe dans la direction du mouvement
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    // Gestion des Collisions (Trigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Vérification si la balle touche le joueur.
        if (other.CompareTag("Player"))
        {
            // Placeholder: Appliquer des dégâts (doit interagir avec un script PlayerHealth)
            Debug.Log($"Balle ennemie touche le joueur! Dégâts infligés : {damageAmount}");
            other.GetComponent<PlayerHealth>()?.TakeDamage(damageAmount);
        }
        
        // Destruction de la balle à l'impact avec n'importe quoi (joueur, mur, environnement).
        // On pourrait ajouter une vérification pour éviter la destruction sur des objets non-pertinents
        
        // Ajout d'une vérification simple pour ne pas détruire si c'est un autre projectile
        if (!other.CompareTag("EnemyBullet"))
        {
            Destroy(gameObject); 
        }
    }
}
