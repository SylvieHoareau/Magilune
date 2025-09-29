using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Swordsman : MonoBehaviour, AttackEventHandler.IDamageable
{
    [Header("Stats")]
    [SerializeField] private int health = 3;
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private int damage = 1;

    [Header("Patrol")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    private bool movingRight = true;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 1.0f;
    [SerializeField] private LayerMask playerLayer;


    [Header("Attack")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 1f;
    private float lastAttackTime;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;

    private enum State { Patrol, Chase, Attack };
    private State currentState = State.Patrol;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    private void Update()
    {
        // Vérifie si le joueur est dans la scène
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                return; // Sort de la méthode Update si le joueur n'est pas trouvé  
            }
        }

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                ChaseP();
                break;
            case State.Attack:
                Attack();
                break;
        }

        // Transition des états
        HandleStateTransitions();

    }

    private void Patrol()
    {
        float dir = movingRight ? 1 : -1;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);

        if (movingRight && transform.position.x > rightPoint.position.x)
            movingRight = false;
        else if (!movingRight && transform.position.x < leftPoint.position.x)
            movingRight = true;

        animator.SetBool("IsMoving", true);
    }

    private void Chase()
    {
        if (player == null) return;

        float direction = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

        animator.SetBool("IsMoving", true);
    }

    private void Attack()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("IsMoving", false);

        if (Time.time - lastAttackTime > attackCooldown)
        {
            animator.SetTrigger("Attack");
            lastAttackTime = Time.time;
        }
    }

    private void HandleStateTransitions()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            currentState = State.Attack;
        }
        else if (distance <= detectionRange)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Patrol;
        }

        // Flip du sprite
        if (player != null && currentState != State.Patrol)
        {
            if (player.position.x < transform.position.x)
                transform.localScale = new Vector3(-1, 1, 1);
            else
                transform.localScale = new Vector3(1, 1, 1);
        }
    }

    // Implémentation de IDamageable
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log($"Swordsman took {damageAmount} damage, remaining health: {health}");

        if (health <= 0)
        {
            Die();
        }
    }
    
    // Animation Event → pour appliquer les dégâts pendant l'attaque
    public void DealDamage()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (hit != null)
        {
            Debug.Log("Player touché !");
            // Ici tu appelles PlayerHealth.TakeDamage(damage)
        }
    }

    public void Die()
    {
        Debug.Log("Swordsman died.");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

}
