using UnityEngine;

public class AttackEventHandler : MonoBehaviour
{
    [Header("Attack Configuration")]
    [Tooltip("La zone d'effet de l'attaque, relative au point de pivot du joueur")]
    [SerializeField] private Vector2 _attackOffset = new Vector2(0.8f, 0.0f);
    [SerializeField] private Vector2 _attackSize = new Vector2(1.2f, 1.0f);
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private int _damageAmount = 1;

    // Référence au SpriteRenderer pour vérifier la direction
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponentInParent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer manquant sur le parent de AttackEventHandler.");
        }
    }

    // Cette méthode sera appelée par un Animation Event !
    public void PerformAttackHit()
    {
        // Calcul la position réelle de la Hitbox
        float direction = _spriteRenderer.flipX ? -1f : 1f;
        Vector2 position = (Vector2)transform.position + new Vector2(_attackOffset.x * direction, _attackOffset.y);

        // Chercher les ennemis dans la zone d'attaque
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(position, _attackSize, 0f, _enemyLayer);

        // Appliquer les dégâts à chaque ennemi touché
        foreach (Collider2D enemy in hitEnemies)
        {
            // Utiliser une interface IDamageable pour plus de flexibilité
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Appeler la méthode TakeDamage de l'ennemi
                damageable.TakeDamage(_damageAmount);

                Debug.Log($"Ennemi {enemy.name} touché pour {_damageAmount} dégâts.");
            }
            else
            {
                Debug.LogWarning($"L'objet {enemy.name} n'implémente pas IDamageable.");
            }
        }

        // Optionnel : Dessiner la boîte pour le debugging dans l'Editor
        DrawBox(position, _attackSize); 

    }

    // Fonction d'aide pour le debug (sera visible dans la vue Scene)
    private void OnDrawGizmosSelected()
    {
        // Vérification de sécurité pour éviter les erreurs si l'objet n'est pas configuré
        if (!Application.isEditor || transform == null)
        {
            return;
        }
           

        // Si le SpriteRenderer n'est pas encore initialisé (ex: lors du premier chargement)
        // on tente de le récupérer, sinon on assume qu'il est là si on est déjà en mode Play/Edit
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null) return;
        }

        // Visualisation de la Hitbox en temps réel dans l'éditeur
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // Orange semi-transparent
        
        // 1. Déterminer la direction actuelle de l'attaque
        // La Hitbox doit se retourner quand le sprite est flipX
        float direction = _spriteRenderer.flipX ? -1f : 1f;

        // 2. Calculer la position centrale de la boîte
        Vector3 position = (Vector2)transform.position + new Vector2(_attackOffset.x * direction, _attackOffset.y);
        

        // 3. Dessiner la boîte de Gizmo
        // Choisir une couleur pour la visualisation
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f); // Orange semi-transparent
        
        // Dessine uniquement le contour de la boîte (WireCube)
        Gizmos.DrawWireCube(center, _attackSize);
        
        // Alternative : Dessiner une boîte pleine (DrawCube) pour une meilleure visibilité, mais attention à ne pas cacher le joueur
        // Gizmos.DrawCube(center, _attackSize);
    }

    // Interface pour les objets pouvant recevoir des dégâts
    public interface IDamageable
    {
        void TakeDamage(int damage);
    }

}
