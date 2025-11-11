using System.Drawing;
using UnityEngine;

public class PatrolModule : MonoBehaviour
{

    [Header("Stats")]
    [SerializeField] private float moveSpeed;

    public Transform[] points;
    public Transform currentPoint;
    public int currentPointNumber;

    public float chaseRange;
    public Transform player;

    public bool isActive = false;

    private Rigidbody2D rb;
    private Animator animator;


    // --- LIFECYCLE ---
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        UpdatePatrolPoint();
    }

    public void Update()
    {
        if (currentPoint != null)
        {
            Patrol();
        }

        // ChaseOrPatrol();
    }

    /// <summary>
    /// Définit l'état actif du module. Utile pour la FSM de EnemyCore.
    /// </summary>
    public void SetModuleActive(bool state)
    {
        isActive = state;
        if (state == false)
        {
            // Logique de sortie : arrêter le mouvement forcé
            // Si le module a un Rigidbody, l'arrêter ici
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // Utilisez la référence 'rb' (Rigidbody2D)
            }        
        }
    }


    public void UpdatePatrolPoint()
    {
        currentPointNumber++;

        if (currentPointNumber >= points.Length - 1)
        {
            currentPointNumber = 0;
        }

        currentPoint = points[currentPointNumber];
    }

    // Fonction pour calculer la distance entre le point et l'ennemi
    public float PointDistance()
    {
        float distance = Vector2.Distance(transform.position, currentPoint.position);
        return distance;
    }

    public void Patrol()
    {
        if (!isActive) return; // Ne rien faire si inactif
        
        animator.SetBool("IsMoving", true);

        transform.position = Vector2.MoveTowards(transform.position, points[currentPointNumber].position, moveSpeed * Time.deltaTime);

        if (PointDistance() < 0.2f)
        {
            UpdatePatrolPoint();
        }
    }

    /// <summary>
    /// Déplace l'ennemi vers le joueur si dans la portée de poursuite.
    /// Cette méthode est appelée depuis EnemyCore.
    /// </summary>
    /// <param name="target">La position du joueur.</param>
    public void Chase(Transform target)
    {
        if (!isActive) return; // Ne rien faire si inactif

        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }

        // Déplacer l'ennemi vers le joueur si dans la portée de poursuite
        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        // Gérer l'orientation de l'ennemi (s'il doit faire face à la cible)
        LookAtTarget(target);
    }
    
    /// <summary>
    /// Oriente l'ennemi pour qu'il fasse face à la cible (joueur ou point de patrouille).
    /// </summary>
    /// <param name="target">Le Transform de la cible à regarder.</param>
    public void LookAtTarget(Transform target)
    {
        if (target == null) return;

        // Calculer la direction horizontale
        // On prend la différence en X entre la cible et l'ennemi.
        float directionX = target.position.x - transform.position.x;

        // Vérifier si la direction est significative (éviter les micro-changements inutiles)
        if (Mathf.Abs(directionX) > 0.01f) // Utiliser une petite tolérance
        {
            // Appliquer l'orientation
            // Mathf.Sign(directionX) retourne +1 si la cible est à droite, -1 si elle est à gauche.
            // On modifie seulement l'échelle X du transform pour retourner le sprite.
            Vector3 newScale = transform.localScale;
            newScale.x = Mathf.Sign(directionX);
            transform.localScale = newScale;
        }
    }

    // public void ChaseOrPatrol()
    // {
    //     if (ChaseDistance() <= chaseRange)
    //     {
    //         // Logique de poursuite du joueur
    //         Debug.Log("Chasing the player!");
    //         transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    //     }
    //     else
    //     {
    //         // Logique lorsque le joueur est hors de portée
    //         Debug.Log("Player is out of range, patrolling.");
    //     }
    // }

    // public float ChaseDistance()
    // {
    //     float distance = Vector2.Distance(transform.position, player.position);
    //     return distance;
    // }

}

