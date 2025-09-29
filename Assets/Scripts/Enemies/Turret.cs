using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Turret : MonoBehaviour, AttackEventHandler.IDamageable
{
    [Header("Stats")]
    [SerializeField] private int health = 3;
    [SerializeField] private int damage = 1;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float fireCooldown = 2f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask playerLayer;

    private Animator animator;
    private Transform player;
    private float lastFireTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Si le joueur est dans la zone de détection
        if (distance <= detectionRange)
        {
            // Orientation vers le joueur
            if (player.position.x < transform.position.x)
                transform.localScale = new Vector3(-1, 1, 1);
            else
                transform.localScale = new Vector3(1, 1, 1);

            // Tir
            if (Time.time - lastFireTime >= fireCooldown)
            {
                Shoot();
                lastFireTime = Time.time;
            }
        }
    }

    private void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            animator.SetTrigger("Shoot");

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // Vitesse selon orientation de la tourelle
            float direction = transform.localScale.x;
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = new Vector2(direction * 5f, 0);

            Destroy(bullet, 3f);
        }
    }

    // Implémentation de IDamageable
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log($"Turret took {damageAmount} damage, remaining health: {health}");

        StartCoroutine(DamageFlash());

        if (health <= 0)
        {
            Die();
        }
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

    private void Die()
    {
        Debug.Log("Turret destroyed.");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
