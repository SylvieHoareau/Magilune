using UnityEngine;
using System.Collections;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Health")]
    public float maxHealth = 3f;
    public float MaxHealth => maxHealth; // Propriété publique en lecture seule
    private float currentHealth;
    public float CurrentHealth => currentHealth; // Propriété publique en lecture seule

    // ÉVÉNEMENTS POUR LA MORT ET LES DÉGÂTS
    public event Action OnEnemyDied; 
    public event Action OnHurt; // Ajout d'un événement pour les dégâts subis
                                // D'autres scripts peuvent s'abonner à ceci
    
    private Animator animator;
    void Awake()
    {
        currentHealth = maxHealth;
        Debug.Log($"Enemy {gameObject.name} initialized with {currentHealth} health.");
    }

    public void EnemyTakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); 
        
        Debug.Log($"Enemy took {damage} damage, remaining health: {currentHealth}");

        // DÉCLENCHEMENT DE L'ÉVÉNEMENT DE DÉGÂTS
        OnHurt?.Invoke(); 

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Gérer la mort de l'ennemi
    public void Die()
    {
        Debug.Log("Enemy died!");

        // DÉCLENCHEMENT DE L'ÉVÉNEMENT DE MORT
        OnEnemyDied?.Invoke(); 

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // La destruction doit rester ici car elle est liée aux données (la vie est finie)
        Destroy(gameObject, 1f); 
        Debug.Log($"Enemy {gameObject.name} will be destroyed in 1 second.");
    }
}
