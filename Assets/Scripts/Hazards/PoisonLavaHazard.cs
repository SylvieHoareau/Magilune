using UnityEngine;

public class PoisonLavaHazard : MonoBehaviour
{
    [SerializeField] private int damagePerSecond = 1;
    [SerializeField] private float damageInterval = 1f; // Intervalle entre les dégâts

    private float lastDamageTime;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Appliquer les dégâts à intervalles réguliers
            if (Time.time - lastDamageTime >= damageInterval)
            {
                // Appliquer les dégâts au joueur
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damagePerSecond);
                    lastDamageTime = Time.time;
                }
            }
        }
    }
    
}
