using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Gunner : MonoBehaviour, AttackEventHandler.IDamageable
{
    [Header("Stats")]
    [SerializeField] private int health = 3;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int damage = 1;

    [Header("Patrol")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    private bool movingRight = true;

    [Header("Ranged Attack Module")]
    // Lien avec le module générique.
    [SerializeField] private RangedAttackModule rangedAttackModule; 

    [Header("Detection & Attack")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float fireCooldown = 1.5f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask playerLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private float lastFireTime;

    private enum State { Patrol, Attack }
    private State currentState = State.Patrol;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;

            case State.Attack:
                Attack();
                break;
        }

         if (currentState == State.Attack)
        {
            if (Time.time > lastFireTime + fireCooldown)
            {
                // Appel de la méthode Shoot du module externe
                // 1. Déclenchement de l'animation de tir
                animator.SetTrigger("Shoot"); 

                // 2. Délégation de l'action de tir au module !
                if (rangedAttackModule != null)
                {
                    rangedAttackModule.Shoot();
                }
                else
                {
                    Debug.LogWarning("RangedAttackModule n'est pas assigné sur le Gunner.");
                }
            }
        }

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

    private void Attack()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("IsMoving", false);

        if (Time.time - lastFireTime >= fireCooldown)
        {
            animator.SetTrigger("Shoot");
            Shoot();
            lastFireTime = Time.time;
        }
    }

    private void HandleStateTransitions()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectionRange)
            currentState = State.Attack;
        else
            currentState = State.Patrol;

        // Flip du sprite vers le joueur
        if (player != null)
        {
            if (player.position.x < transform.position.x)
                transform.localScale = new Vector3(-1, 1, 1);
            else
                transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void Shoot()
    {
        Debug.Log($" {gameObject.name} attaque à distance !");

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // Détermine la direction du tir
            float direction = transform.localScale.x;
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
                bulletRb.linearVelocity = new Vector2(direction * 5f, 0); // 5f = vitesse du projectile

            // On peut détruire la balle après un certain temps
            Destroy(bullet, 3f);
        }

         if (bulletPrefab == null)
        {
            Debug.Log("Pas de bullet Prefab assignée au Gunner, impossible de tirer.");
            return;
        }
    }

    // Implémentation de IDamageable
    public void EnemyTakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log($"Gunner took {damageAmount} damage, remaining health: {health}");

        // Flash rouge
        StartCoroutine(DamageFlash());

        if (health <= 0) Die();
    }

    private System.Collections.IEnumerator DamageFlash()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            sr.color = original;
        }
    }

    // Gère la mort du Gunner
    private void Die()
    {
        Debug.Log("Gunner died.");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
