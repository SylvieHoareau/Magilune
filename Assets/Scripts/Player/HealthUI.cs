using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartEmpty;
    [SerializeField] private Image[] hearts; // Tableau contenant les 3 cœurs

    /// <summary>
    /// Met à jour l’affichage des cœurs en fonction des PV du joueur.
    /// </summary>
    public void UpdateHearts(float currentHealth, float maxHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].sprite = heartFull;
            }
            else
            {
                hearts[i].sprite = heartEmpty;
            }

            // Optionnel : cacher les cœurs au-delà du maxHealth
            hearts[i].enabled = (i < maxHealth);
        }
    }
}
