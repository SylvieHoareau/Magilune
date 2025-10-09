using UnityEditorInternal;
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
        GetComponent<EnemyHealth>().OnEnemyDied += TransitionToDie;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        if (player == null) return;

        TransitionTo(currentState); // Appel de la méthode de transition chaque frame
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

    /// <summary>
    /// Transitionne vers un nouvel état.
    /// </summary>
    /// <param name="newState"></param>
    public void TransitionTo(EnemyState newState)
    {
        if (player == null) return;

        // Détermine la distance par rapport au joueur (Optimisation : ne pas faire dans Update si possible)
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Logique de sortie de l'état précédent si nécessaire
        switch (currentState)
        {
            case EnemyState.Patrol:
            case EnemyState.Chase:
                // PatrolModule gère à la fois la Patrouille et la Poursuite.
                if (patrolModule != null) patrolModule.SetModuleActive(false);
                break;
            case EnemyState.Attack:
                // Désactiver les deux modules d'attaque pour s'assurer qu'ils s'arrêtent.
                if (meleeAttackModule != null) meleeAttackModule.SetModuleActive(false);
                if (rangedAttackModule != null) rangedAttackModule.SetModuleActive(false);
                break;
            case EnemyState.Hurt:
            case EnemyState.Die:
                // Rien à désactiver en particulier ici.
                break;
        }

        // Mise à jour de l'état actuel
        currentState = newState;

        // Synchronisation avec l'Animator
        if (animator != null)
        {
            // int correspond à l'index de l'énumération (0=Idle, 1=Patrol, 2=Chase, 3=Attack, 4=Hurt, 5=Die)
            animator.SetInteger("State", (int)currentState);
        }
        // Logique d'entrée dans le nouvel état si nécessaire
        switch (newState)
        {
            case EnemyState.Patrol:
            case EnemyState.Chase:
                // Initialiser la patrouille et de la poursuite
                // PatrolModule gère à la fois la Patrouille et la Poursuites
                if (patrolModule != null) patrolModule.SetModuleActive(true);               
                break;
            case EnemyState.Attack:
                // Initialiser l'attaque
                // Désactiver les deux modules d'attaque pour sassurer qu'ils s'arrêtent
                if (meleeAttackModule != null) meleeAttackModule.SetModuleActive(true);
                else if (rangedAttackModule != null) rangedAttackModule.SetModuleActive(true);
                break;
            case EnemyState.Hurt:
                // Initialiser les dégâts
                animator.SetTrigger("Hurt");
                break;
            case EnemyState.Die:
                // Initialiser la mort
                animator.SetTrigger("Die");
                // Désactiver le Core pour arrêter tout mouvement/logique
                this.enabled = false;
                break;
        }

        Debug.Log($"Transition vers l'état : {currentState}");

    }
    
    public void TakeHit()
    {
        // On ne veut pas se faire frapper si on est déjà mort
        if (currentState == EnemyState.Die) return;

        // Si on n'est pas déjà en train de subir des dégâts pour éviter de casser l'animation Hurt
        if (currentState != EnemyState.Hurt)
        {
            TransitionTo(EnemyState.Hurt);
        }
    }

    public void ResumeState()
    {
        // Après avoir été blessé, on ne reprend pas l'état Hurt/Die.
        // On revient à la poursuite si le joueur est toujours là, sinon à la patrouille.
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= detectionRange)
        {
            TransitionTo(EnemyState.Chase);
        }
        else
        {
            TransitionTo(EnemyState.Patrol);
        }
    }
    
    public void TransitionToDie()
    {
        TransitionTo(EnemyState.Die);
        // Désactiver le Core pour arrêter tout mouvement/logique
        this.enabled = false;
    }
}
