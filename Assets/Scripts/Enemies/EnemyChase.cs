using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    public float chaseRange;
    public Transform player;

    public float moveSpeed = 2f;

    // -----------------LIFE CYCLE-----------------

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ChaseOrPatrol();
    }

    // -----------------METHODS-----------------

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
