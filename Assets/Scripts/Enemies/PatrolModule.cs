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

    private Rigidbody2D rb;
    private Animator animator;

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
        float direction = target.position.x - transform.position.x;
        if (Mathf.Abs(direction) > 0.1f)
        {
            // Change l'échelle pour retourner le sprite : +1 si à droite, -1 si à gauche
            transform.localScale = new Vector3(Mathf.Sign(direction), 1, 1);
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

