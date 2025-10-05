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

    public event Action OnDeath; // C'est un System.Action, un événement sans paramètre
    public event Action OnHurt; // Ajout d'un événement pour les dégâts subis
    // D'autres scripts peuvent s'abonner à ceci
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
        OnDeath?.Invoke(); 
        
        // La destruction doit rester ici car elle est liée aux données (la vie est finie)
        Destroy(gameObject, 1f); 
        Debug.Log($"Enemy {gameObject.name} will be destroyed in 1 second.");
    }
}
