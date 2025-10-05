using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 1f;           // dégâts infligés
    public Vector3 playerPos; // Position du joueur pour orienter la balle

    public float bulletSpeed = 10f; // Vitesse de la balle

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, 3f); // Détruire la balle après 3 secondes pour éviter les fuites mémoire
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, playerPos, bulletSpeed * Time.deltaTime);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject); // Détruire la balle après l'impact
        }
    }
}
