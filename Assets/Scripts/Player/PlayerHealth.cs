using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    // Propriété publique pour vérifier si le joueur est mort
    public bool IsDead => currentHealth <= 0;

    [Header("Game Over")]
    [SerializeField] private float deathAnimationTime = 1.5f; // Durée de l'animation de mort
    [SerializeField] private string gameOverSceneName = "GameOver"; // Nom de la scène à charger

    private Animator animator;
    private HealthUI healthUI;

    void Awake()
    {
        // Initialisation de la santé
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        // Trouver le HealthUI dans la scène
        healthUI = FindObjectOfType<HealthUI>();

        if (healthUI != null)
        {
            // Initialiser l'UI de santé
            healthUI.UpdateHearts(currentHealth, maxHealth);
        }
        else
        {
            Debug.LogWarning("HealthUI non trouvé dans la scene. Ajouter HealthUI component.");
        }
    }

    /// <summary>
    /// Méthode appelée par les ennemis (comme Swordsman) pour infliger des dégâts au joueur
    /// </summary>
    public void TakeDamage(int damage)
    {
        Debug.Log("Player Script TakeDamage called");
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Empêche la santé de descendre en dessous de 0   
        Debug.Log($"Player took {damage} damage, remaining health: {currentHealth}");

        // Feedback visuel/sonore
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }

        // Mettre à jour l'UI de santé
        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Méthode pour la mort du joueur
    private void Die()
    {
        Debug.Log("Player died!");

        // Déclenchement de l'animation de mort
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

       // Désactiver les contrôles immédiatement (pour éviter toute interaction)
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        // Démarrer la séquence de fin de jeu
        StartCoroutine(GameOverSequence());
    }

    /// <summary>
    /// Coroutine pour temporiser l'affichage de l'écran Game Over.
    /// </summary>
    private IEnumerator GameOverSequence()
    {
        // 1. Attendre que l'animation de mort soit jouée
        yield return new WaitForSeconds(deathAnimationTime);

        // 2. Charger la scène Game Over
        // PRO-TIP : Vérifiez toujours que la scène existe
        if (Application.CanStreamedLevelBeLoaded(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            Debug.LogError($"Scène Game Over non trouvée ou non ajoutée aux Build Settings : {gameOverSceneName}");
            // Fallback : Recharger la scène actuelle si la scène Game Over est manquante
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // Méthode pour soigner le joueur (optionnelle)
    // public void Heal(int amount)
    // {
    //     currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    //     Debug.Log($"Player healed, new health: {currentHealth}");
    //     if (healthUI != null)
    //     {
    //         healthUI.UpdateHearts(currentHealth, maxHealth);
    //     }
    // }
}
