using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
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

    /// <summary>
    /// Méthode appelée par les ennemis (comme Swordsman) pour infliger des dégâts
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
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

    private void Die()
    {
        Debug.Log("Player died!");
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // On peut désactiver les contrôles
        GetComponent<PlayerController>().enabled = false;

        // TODO : ajouter écran de Game Over
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Player healed, new health: {currentHealth}");
        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth, maxHealth);
        }
    }
}
