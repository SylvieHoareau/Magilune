using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    [SerializeField] public float damage = 1f;           // dégâts infligés
    [SerializeField] private float lifetime = 3f;      // durée avant auto-destruction
    [SerializeField] private LayerMask hitLayers;      // couches que la balle peut toucher (Player, murs)

    public Vector3 playerPos; // Position du joueur pour orienter la balle

    private void Start()
    {
        // Détruire automatiquement après X secondes pour éviter les fuites mémoire
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Vérifier si l’objet touché est dans le layer autorisé
        if (((1 << collision.gameObject.layer) & hitLayers) != 0)
        {
            // Si c’est le Player → on applique des dégâts
            if (collision.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }

            // Vérifie si l’objet peut recevoir des dégâts
            AttackEventHandler.IDamageable damageable = collision.GetComponent<AttackEventHandler.IDamageable>();
            if (damageable != null)
            {
                damageable.EnemyTakeDamage(damage);
            }


            // Détruire la balle après l’impact
            Destroy(gameObject);
        }
    }
}
