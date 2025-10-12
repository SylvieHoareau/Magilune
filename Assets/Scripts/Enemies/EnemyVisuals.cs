// EnemyVisuals.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyHealth))] // S'assure d'avoir la source d'événements
public class EnemyVisuals : MonoBehaviour
{
    [Header("Composants Visuels")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Feedback de Dégâts")]
    [SerializeField] private float damageFlashDuration = 0.15f; 
    [SerializeField] private Color damageFlashColor = Color.red; 
    
    // Références privées
    private EnemyHealth _health;
    private EnemyFloatingHealthUI _floatingUI; 
    private Coroutine _flashCoroutine;

    private static readonly int HurtTrigger = Animator.StringToHash("Hurt");
    private static readonly int DieTrigger = Animator.StringToHash("Die");

    private void Awake()
    {
        // 1. Récupération des composants
        _health = GetComponent<EnemyHealth>();
        // On cherche le SR et l'Animator, soit sur l'objet, soit on assigne dans l'Inspector
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        _floatingUI = GetComponentInChildren<EnemyFloatingHealthUI>();

        // 2. ABONNEMENT aux événements de EnemyHealth !
        _health.OnHurt += HandleHurt;
        _health.OnEnemyDied += HandleDeath;
    }

    private void OnDestroy()
    {
        // TRÈS IMPORTANT : Se désabonner pour éviter les erreurs si l'objet est détruit
        if (_health != null)
        {
            _health.OnHurt -= HandleHurt;
            _health.OnEnemyDied -= HandleDeath;
        }
    }

    // --- Gestionnaires d'Événements ---

    private void HandleHurt()
    {
        // 1. Animation
        if (animator != null)
            animator.SetTrigger(HurtTrigger);

        // 2. Flash de couleur
        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine); 
        _flashCoroutine = StartCoroutine(DamageFlash());

        // 3. UI
        if (_floatingUI != null)
            _floatingUI.UpdateFloatingHeart(_health.MaxHealth, _health.CurrentHealth);
    }

    private void HandleDeath()
    {
        // 1. Animation de mort
        if (animator != null)
            animator.SetTrigger(DieTrigger);
        
        // 2. Désactiver l'UI
        if (_floatingUI != null)
            _floatingUI.gameObject.SetActive(false);
            
        // Optionnel : Désactiver le rendu et les collisions immédiatement si l'animation est gérée ailleurs
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }

    private IEnumerator DamageFlash()
    {
        // ... (Logique de la coroutine DamageFlash) ...
        // Le code reste le même que celui que tu avais
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = damageFlashColor;
        
        yield return new WaitForSeconds(damageFlashDuration);
        
        spriteRenderer.color = originalColor;
        _flashCoroutine = null;
    }
}