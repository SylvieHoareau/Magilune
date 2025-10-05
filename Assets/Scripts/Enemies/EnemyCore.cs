using UnityEngine;

/// <summary>
/// Le script de base pour les ennemis. Peut être étendu pour des comportements spécifiques.
/// </summary>

public enum EnemyState { Idle, Patrol, Chase, Attack, Hurt, Die };

[RequireComponent(typeof(Animator))]
public class EnemyCore : MonoBehaviour
{
    [Header("Module Requis")]
    // Ces modules sont sur l'ennemi et gèrent leurs propres logiques
    [SerializeField] private PatrolModule patrolModule;
    [SerializeField] private MeleeAttackModule meleeAttackModule;
    [SerializeField] private RangedAttackModule rangedAttackModule;

    [SerializeField] private Transform player;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 5f; // Portée de détection du joueur
    [SerializeField] private LayerMask playerLayer; // Le layer du joueur

    private EnemyState currentState = EnemyState.Patrol;
    private Transform playerTransform;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        // Optionnel : lier la fonction Die() à l'événement de mort de EnemyHealth
        GetComponent<EnemyHealth>().OnDeath += TransitionToDie;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        if (player == null) return;

        // 1. Détermine la distance par rapport au joueur (Optimisation : ne pas faire dans Update si possible)
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 2. LOGIQUE GLOBALE DE TRANSITION D'ÉTAT
        switch (currentState)
        {
            case EnemyState.Patrol:
                HandlePatrolState(distanceToPlayer);
                break;
            case EnemyState.Chase:
                HandleChaseState(distanceToPlayer);
                break;
            case EnemyState.Attack:
                HandleAttackState(distanceToPlayer);
                break;
                // Hurt et Die gérés par l'événement de EnemyHealth
        }
    }
    
    // --- Fonctions de transition d'état ---

    private void HandlePatrolState(float dist)
    {
        if (dist <= detectionRange)
        {
            TransitionTo(EnemyState.Chase);
        }
        else if (patrolModule != null)
        {
            patrolModule.Patrol();
        }
    }
    
    private void HandleChaseState(float dist)
    {
        if (dist > detectionRange)
        {
            TransitionTo(EnemyState.Patrol); // Revenir en patrouille
            return;
        }

        if (dist <= meleeAttackModule.AttackRange || dist <= rangedAttackModule.AttackRange)
        {
            TransitionTo(EnemyState.Attack);
            return;
        }

        // Sinon, continuer à chasser
        patrolModule.Chase(player); 
    }

    private void HandleAttackState(float dist)
    {
        // Si plus à portée d'attaque, revenir à la poursuite
        if (dist > meleeAttackModule.AttackRange && dist > rangedAttackModule.AttackRange)
        {
            TransitionTo(EnemyState.Chase);
            return;
        }

        // Attaque (le module gère son propre cooldown)
        if (meleeAttackModule != null)
            meleeAttackModule.TryAttack();
        else if (rangedAttackModule != null)
            rangedAttackModule.TryShoot();
    }
    
    public void TransitionTo(EnemyState newState)
    {
        // Logique de sortie de l'état précédent si nécessaire
        // ...

        currentState = newState;
        animator.SetInteger("State", (int)currentState); // Lier à l'Animator

        // Logique d'entrée dans le nouvel état
        // ...
    }
    
    public void TransitionToDie()
    {
        TransitionTo(EnemyState.Die);
        // Désactiver le Core pour arrêter tout mouvement/logique
        this.enabled = false; 
    }
}
