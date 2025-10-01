using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public Transform player;
    public float damage;
    public GameObject bulletPrefab;
    public float attackRange;

    public enum EnemyType { Melee, Range };
    public EnemyType enemyType;

    public bool canAttack;
    public float attackCooldown = 1f;

    // -----------------LIFE CYCLE-----------------

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerDistance() <= attackRange && canAttack)
        {
            // Logique d'attaque du joueur
            Debug.Log("Attacking the player!");

            // Attaque
            canAttack = false;

            // Ici, vous pouvez ajouter la logique d'attaque spécifique (par exemple, réduire la santé du joueur)
            switch (enemyType)
            {
                case EnemyType.Melee:
                    // Logique d'attaque au corps à corps
                    Debug.Log("Melee attack logic here.");
                    MeleeAttack();
                    break;
                case EnemyType.Range:
                    // Logique d'attaque à distance
                    Debug.Log("Ranged attack logic here.");
                    RangeAttack();
                    break;
            }
            StartCoroutine(AttackCooldown());
        }
        else
        {
            // Logique lorsque le joueur est hors de portée
            Debug.Log("Player is out of attack range.");
        }
    }

    // -----------------METHODS-----------------

    public void MeleeAttack()
    {
        // Logique d'attaque au corps à corps
        Debug.Log("Melee attack executed.");
        player.GetComponent<PlayerHealth>().TakeDamage(damage);

    }

    public void RangeAttack()
    {
        // Logique d'attaque à distance
        Debug.Log("Ranged attack executed.");
        GameObject newBullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        newBullet.GetComponent<Bullet>().damage = damage;
        newBullet.GetComponent<Bullet>().playerPos = player.position;
    }


    public float PlayerDistance()
    {
        float playerDistance = Vector2.Distance(transform.position, player.position);
        return playerDistance;
    }
    
    public IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
