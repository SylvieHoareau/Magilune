using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    private Animator animator;
    private HealthUI healthUI;
    void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        healthUI = FindObjectOfType<HealthUI>();

        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth, maxHealth);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Empêche la santé de descendre en dessous de 0   
        Debug.Log($"Player took {damage} damage, remaining health: {currentHealth}");

        // Feedback visuel/sonore
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }

        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Die()
    {
        Debug.Log("Enemy died!");
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Désactiver les comportements de l'ennemi ici (mouvement, attaque, etc.)
        // ...

        // Détruire l'objet après un délai pour permettre à l'animation de mort de jouer
        Destroy(gameObject, 1f); // Ajuster le délai selon la durée de l'animation
    }
}
