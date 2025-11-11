// EnemyFloatingHealthUI.cs
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyFloatingHealthUI : MonoBehaviour
{
    [Header("Sprites de Coeur")]
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartHalf;
    [SerializeField] private Sprite heartEmpty;
    
    // Position relative à l'ennemi (ajustez dans l'éditeur)
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, 0f); 

    private SpriteRenderer _spriteRenderer;
    private EnemyHealth _enemyHealth; // Référence au script de santé de l'ennemi

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        // On suppose que ce script est un enfant, et que EnemyHealth est sur le parent
        _enemyHealth = GetComponentInParent<EnemyHealth>(); 
        
        if (_spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer manquant sur l'objet de l'UI de vie flottante.");
            enabled = false;
        }

        // Positionnement initial
        transform.localPosition = offset;

        // Mise à jour initiale de l'affichage
        if (_enemyHealth != null)
        {
            UpdateFloatingHeart(_enemyHealth.MaxHealth, _enemyHealth.CurrentHealth);
        }
        else
        {
            Debug.LogError("EnemyHealth script manquant sur le parent. L'UI de vie ne fonctionnera pas.");
            enabled = false;
        }
    }

    /// <summary>
    /// Met à jour l'image du coeur en fonction de la santé de l'ennemi.
    /// </summary>
    /// <param name="maxHealth">Santé maximale de l'ennemi.</param>
    /// <param name="currentHealth">Santé actuelle de l'ennemi.</param>
    public void UpdateFloatingHeart(float maxHealth, float currentHealth)
    {
        // 1. Calculer le ratio de vie restant
        float healthRatio = (float)currentHealth / maxHealth;
        
        // 2. Déterminer l'état (Plein, Moitié, Vide)
        Sprite targetSprite;

        if (healthRatio >= 0.75f) // > 75%
        {
            targetSprite = heartFull;
        }
        else if (healthRatio > 0.1f) // > 10% (pour éviter qu'un cœur à 1/3 soit mi-plein)
        {
            targetSprite = heartHalf;
        }
        else if (healthRatio > 0f) // > 0
        {
            targetSprite = heartEmpty;
        }
        else // Santé = 0
        {
            // Lorsque l'ennemi meurt, désactiver le Sprite pour laisser place à l'animation de mort
            _spriteRenderer.enabled = false; 
            return;
        }

        _spriteRenderer.sprite = targetSprite;
        _spriteRenderer.enabled = true; // Assurez-vous qu'il est visible tant qu'il a de la vie
    }
}