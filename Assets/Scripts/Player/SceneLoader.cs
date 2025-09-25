using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Fonction pour charger la scène suivante
    public void NextScene()
    {

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Fonction pour retourner au menu principal
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Nom exact de la scène dans Build Settings
    }
    
    // Fonction pour charger la scène Crédits
    public void LoadCredits()
    {
        SceneManager.LoadScene("Credits"); // crée une scène "Credits" et ajoute-la dans Build Settings
    }

    // Fonction pour quitter le jeu
    public void QuitGame()
    {
        Debug.Log("Quitter le jeu..."); // utile en mode éditeur
        Application.Quit(); // fonctionne seulement dans un build (pas dans l’éditeur Unity)
    }
}
