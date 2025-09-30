using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Health")]
    [SerializeField] private int maxHealth = 3;
    public int MaxHealth => maxHealth; // Propriété publique en lecture seule
    private int currentHealth;
    public int CurrentHealth => currentHealth; // Propriété publique en lecture seule

    [Header("Hit Effect")]
    private EnemyFloatingHealthUI _floatingUI; 
    private Animator animator;

    [Header("Feedback Visuel (Pour les 3 ans)")]
    [SerializeField] private float damageFlashDuration = 0.15f; // Durée du flash
    [SerializeField] private Color damageFlashColor = Color.red; // Couleur de l'effet
    
    private SpriteRenderer _spriteRenderer;
    private HealthUI healthUI;
    private Coroutine _flashCoroutine;
    void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>(); // Récupérer le SpriteRenderer

        // Trouver l'UI flottante dans les enfants
        _floatingUI = GetComponentInChildren<EnemyFloatingHealthUI>();
        // Mise à jour initiale de l'UI flottante
        if (_floatingUI != null)
        {
            _floatingUI.UpdateFloatingHeart(maxHealth, currentHealth);
        }
        else
        {
            Debug.LogWarning("EnemyFloatingHealthUI non trouvé dans les enfants. L'UI de vie flottante ne fonctionnera pas.");
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Empêche la santé de descendre en dessous de 0   
        Debug.Log($"Player took {damage} damage, remaining health: {currentHealth}");

        // DÉCLENCHER LE FLASH DE COULEUR
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine); // Arrêter l'ancien flash pour que le nouveau puisse démarrer
        }
        _flashCoroutine = StartCoroutine(DamageFlash());

        // Animation
        if (animator != null)
        {
            // Utiliser un trigger est mieux que SetBool si l'animation est courte et ne boucle pas
            animator.SetTrigger("Hurt"); 
        }

        // UI (si l'ennemi a sa propre UI)
        if (_floatingUI != null)
        {
            _floatingUI.UpdateFloatingHeart(maxHealth, currentHealth);
        }

        // Mort
        if (currentHealth <= 0)
        {
            Die();
        }
    }

     /// <summary>
    /// Coroutine pour le feedback visuel de dégâts (Flash rouge/blanc).
    /// </summary>
    private IEnumerator DamageFlash()
    {
        if (_spriteRenderer == null) yield break; // S'assurer que le SR existe

        Color originalColor = _spriteRenderer.color;
        
        // Appliquer la couleur de dégât
        _spriteRenderer.color = damageFlashColor;
        
        yield return new WaitForSeconds(damageFlashDuration);
        
        // Revenir à la couleur originale
        _spriteRenderer.color = originalColor;

        _flashCoroutine = null;
    }
    
    public void Die()
    {
        Debug.Log("Enemy died!");
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // DÉSACTIVER IMMÉDIATEMENT L'UI FLOTTANTE LORS DE LA MORT
        if (_floatingUI != null)
        {
            _floatingUI.gameObject.SetActive(false); 
        }

        // Détruire l'objet après un délai pour permettre à l'animation de mort de jouer
        Destroy(gameObject, 1f); // Ajuster le délai selon la durée de l'animation
    }
}
