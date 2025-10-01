using System.Drawing;
using UnityEngine;

public class EnnemyPatrol : MonoBehaviour
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

        ChaseOrPatrol();
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
    
    public void Chase()
    {
        if (ChaseDistance() <= chaseRange)
        {
            // Logique de poursuite du joueur
            Debug.Log("Chasing the player!");
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
        else
        {
            // Logique lorsque le joueur est hors de portée
            Debug.Log("Player is out of range.");
        }
    }

    public void ChaseOrPatrol()
    {
        if (ChaseDistance() <= chaseRange)
        {
            // Logique de poursuite du joueur
            Debug.Log("Chasing the player!");
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
        else
        {
            // Logique lorsque le joueur est hors de portée
            Debug.Log("Player is out of range, patrolling.");
        }
    }

    public float ChaseDistance()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        return distance;
    }

}

