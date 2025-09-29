using UnityEngine;

public class LevelExit : MonoBehaviour
{
    [SerializeField] private string nextLevelName;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Charger le niveau suivant
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelName);
            Debug.Log($"Niveau '{nextLevelName}' chargé.");
        }
    }
}
