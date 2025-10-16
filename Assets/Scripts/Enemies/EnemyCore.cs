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

    // Référence au Player, trouvé via Tag
    private Transform player;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 5f; // Portée de détection du joueur
    [SerializeField] private LayerMask playerLayer; // Le layer du joueur

    private EnemyState currentState = EnemyState.Patrol;
    private Animator animator;
    private float _distanceToPlayer; // Variable pour optimiser le calcul de distance

    private void Awake()
    {
        animator = GetComponent<Animator>();
        // Trouver le joueur par Tag
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // S'assurer que le script EnemyHealth existe

        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.OnEnemyDied += TransitionToDie;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (player == null || currentState == EnemyState.Die) return;

        // Calculer la distance au joueur une seule fois par frame (Optimisation)
        _distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Exécuter la logique de l'état ACTUEL
        ExecuteCurrentStateLogic();
    }

    /// <summary>
    /// Exécute la logique associée à l'état actuel (l'ancienne méthode TransitionTo simplifiée).
    /// </summary>
    private void ExecuteCurrentStateLogic()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                HandlePatrolState(_distanceToPlayer);
                break;
            case EnemyState.Chase:
                HandleChaseState(_distanceToPlayer);
                break;
            case EnemyState.Attack:
                HandleAttackState(_distanceToPlayer);
                break;
            case EnemyState.Hurt:
                // L'état Hurt attend l'animation de fin via un appel à ResumeState().
                break;
            // Die est géré par le 'return' en haut de Update().
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

        // Vérification de la portée d'attaque (la logique de tir est dans HandleAttackState)
        float range = (rangedAttackModule != null) 
            ? rangedAttackModule.AttackRange // Si Ranged, utiliser sa portée
            : (meleeAttackModule != null) 
                ? meleeAttackModule.AttackRange // Sinon, utiliser Melee
                : 0f; // Si aucun module

        if (dist <= range)
        {
            TransitionTo(EnemyState.Attack);
            return;
        }

        // Sinon, continuer à chasser
        if (patrolModule != null)
        {
            patrolModule.Chase(player); 
        }
    }

    private void HandleAttackState(float dist)
    {
        // On quitte l'état Attack si le joueur sort de la portée d'attaque maximale.
        float maxAttackRange = 0f;
        if (meleeAttackModule != null) maxAttackRange = Mathf.Max(maxAttackRange, meleeAttackModule.AttackRange);
        
        // On utilise la DÉTECTION comme portée de tir maximale.
        if (rangedAttackModule != null) maxAttackRange = Mathf.Max(maxAttackRange, detectionRange); // Fallback: DetectionRange
        
        if (dist > maxAttackRange)
        {
            TransitionTo(EnemyState.Chase);
            return;
        }

        // Le Gunner doit regarder la cible avant de tirer.
        if (patrolModule != null)
        {
            patrolModule.LookAtTarget(player);
        }
        
        // --- LOGIQUE DE TIR CORRIGÉE ---
        
        // Calculer la direction de visée vers le joueur (pour le Gunner)
        Vector2 fireDirection = (player.position - rangedAttackModule.firePoint.position).normalized; 
        
        // Tenter d'attaquer
        if (meleeAttackModule != null && dist <= meleeAttackModule.AttackRange)
        {
            // Le Melee Attack Module n'a pas besoin de direction, il attaque frontalement
            meleeAttackModule.TryAttack();
        }
        else if (rangedAttackModule != null) // Pas de mêlée, ou hors de portée mêlée
        {
            // Tenter de tirer avec la direction calculée (le changement majeur !)
            rangedAttackModule.TryShoot(fireDirection);
        }
    }

    /// <summary>
    /// Transitionne vers un nouvel état.
    /// </summary>
    /// <param name="newState"></param>
    public void TransitionTo(EnemyState newState)
    {
        if (currentState == newState) return; // Evite les transitions inutiles

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
