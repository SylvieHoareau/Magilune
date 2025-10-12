using UnityEngine;
public class ScoreManager : MonoBehaviour
{
    private int score = 0;
    public int Score => score; // Propriété publique en lecture seule
    private void Awake()
    {
        // Optionnel : Trouver tous les ennemis de la scène et s'abonner.
        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();

        foreach (EnemyHealth enemy in enemies)
        {
            // L'abonnement se fait ici (ou à la ligne 23 dans votre cas)
            enemy.OnEnemyDied += AddScoreOnDeath;
        }
    }

    // Méthode appelée lorsqu'un ennemi est détruit
    private void OnEnable()
    {
        // On récupère la référence d'une façon ou d'une autre (ex: de la collision)
        EnemyHealth enemyHealth = GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {
            // S'ABONNER : Quand l'événement OnEnemyDied est appelé, la méthode HandleEnemyDeath l'est aussi
            enemyHealth.OnEnemyDied += HandleEnemyDeath;
        }
    }

    private void OnDisable()
    {
        // TOUJOURS SE DÉSABONNER pour éviter les fuites mémoire (Garbage Collection)
        EnemyHealth enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.OnEnemyDied -= HandleEnemyDeath;
        }
    }

    private void HandleEnemyDeath()
    {
        Debug.Log("Réaction à la mort : L'ennemi va dropper un objet !");
        // Logique pour instancier un objet à looter ici.
    }

    private void AddScoreOnDeath()
    {
        // Logique pour augmenter le score
        Debug.Log("Ennemi tué ! +100 points !");
        score += 100;
    }
}