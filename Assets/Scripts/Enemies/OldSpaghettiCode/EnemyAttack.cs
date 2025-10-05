using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public Transform player;
    public float damage;
    public GameObject bulletPrefab;
    public float attackRange;

    public enum EnemyType { Melee, Range };
    public EnemyType enemyType;

    public bool canAttack;
    public float attackCooldown = 1f;

    [Header("Melee Hitbox")]
    [SerializeField] private float hitBoxRadius = 0.5f; // Rayon de la zone de coup
    [SerializeField] private LayerMask playerLayer; // Le layer du joueur
    [SerializeField] private Animator animator; // L'animator de l'ennemi
    const string MeleeTrigger = "Attack"; // Parce que j'utilise un Trigger dans l'animation

    // -----------------LIFE CYCLE-----------------

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerDistance() <= attackRange && canAttack)
        {
            // Logique d'attaque du joueur
            Debug.Log("Attacking the player!");

            // Attaque
            canAttack = false;

            // Ici, vous pouvez ajouter la logique d'attaque spécifique (par exemple, réduire la santé du joueur)
            switch (enemyType)
            {
                case EnemyType.Melee:
                    // Logique d'attaque au corps à corps
                    Debug.Log("Melee attack logic here.");
                    MeleeAttack();
                    break;
                case EnemyType.Range:
                    // Logique d'attaque à distance
                    Debug.Log("Ranged attack logic here.");
                    RangeAttack();
                    break;
            }
            StartCoroutine(AttackCooldown());
        }
        else
        {
            // Logique lorsque le joueur est hors de portée
            Debug.Log("Player is out of attack range.");
        }
    }

    // -----------------METHODS-----------------

    public void MeleeAttack()
    {
        // Déclenche l'animation d'attaque
        animator.SetTrigger(MeleeTrigger);

        // Utilise Physics2D.OverlapCircle pour vérifier les dégâts
        // Le point central de la Hitbox est généralement devant l'ennemi.
        Vector2 hitPoint = (Vector2)transform.position + new Vector2(
            (transform.localScale.x > 0 ? 1 : -1) * hitBoxRadius, 0
        );

        // Récupérer tous les colliders dans la zone de frappe
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(hitPoint, hitBoxRadius, playerLayer);

        foreach (Collider2D target in hitTargets)
        {
            // On s'assure que c'est bien le joueur
            if (target.CompareTag("Player"))
            {
                PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log($"Dégâts Melee appliqués au joueur : {damage}");
                }
            }
        }

    }

    public void RangeAttack()
    {
        // Logique d'attaque à distance
        Debug.Log("Ranged attack executed.");
        GameObject newBullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        newBullet.GetComponent<Bullet>().damage = damage;
        newBullet.GetComponent<Bullet>().playerPos = player.position;
    }


    public float PlayerDistance()
    {
        float playerDistance = Vector2.Distance(transform.position, player.position);
        return playerDistance;
    }

    public IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Dessine la zone de frappe dans l'éditeur (visible uniquement quand l'objet est sélectionné)
        Vector2 hitPoint = (Vector2)transform.position + new Vector2(
            (transform.localScale.x > 0 ? 1 : -1) * hitBoxRadius, 0
        );
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPoint, hitBoxRadius);
    }
}
