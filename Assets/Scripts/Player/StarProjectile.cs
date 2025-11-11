// StarProjectile.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] 
public class StarProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private int damage = 1;

    private Vector2 direction; // La direction de l'ennemi ciblé

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is missing from the StarProjectile.");
        }
    }


    /// <summary>
    /// Initialise et propulse le projectile étoile.
    /// </summary>
    /// <param name="targetDirection">La direction normalisée vers laquelle lancer l'étoile.</param>
    public void Launch(Vector2 targetDirection)
    {
        // 1. Appliquer directement la vitesse au Rigidbody2D
        // C'est l'équivalent le plus direct et performant du "lancement".
        rb.linearVelocity = targetDirection.normalized * speed;

        // 2. Rotationner l'étoile (cosmétique)
        float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // 3. Auto-destruction
        Destroy(gameObject, 2f); // Se détruit après 2 secondes max
    }

    // Appelée par l'AttackEventHandler au moment de l'instanciation
    public void Initialize(Vector2 targetDirection)
    {
        direction = targetDirection.normalized; // Normaliser pour une vitesse constante
        // Rotationner l'étoile pour faire face à la direction (optionnel)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Assurer que le projectile se détruit après un certain temps ou distance
        Destroy(gameObject, 2f); // Se détruit après 2 secondes max
    }

    // void Update()
    // {
    //     // Déplacement cinématique simple et rapide
    //     transform.Translate(direction * speed * Time.deltaTime, Space.World);
    // }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Détecter l'ennemi (ajuster le Tag/Layer)
        if (other.CompareTag("Enemy"))
        {
            // Appliquer les dégâts (méthode de l'ennemi)
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.EnemyTakeDamage(damage);
            }

            // Effet d'impact visuel ici (explosion de particules)

            Destroy(gameObject); // L'étoile disparaît à l'impact
        }
    }
}