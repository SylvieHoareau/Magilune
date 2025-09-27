using UnityEngine;
using System.Collections; // Nécessaire pour le Coroutine

/// <summary>
/// Gère les capacités actuelles du joueur et leur état (activé/désactivé).
/// </summary>
public class PlayerAbilityManager : MonoBehaviour
{
    // Rendre l'état public pour que d'autres scripts puissent le lire (ex: PlayerController)
    [field: SerializeField] public bool CanJump { get; private set; } = true;
    
    // Événement pour signaler la perte de capacité au reste du système (Caméra, UI, etc.)
    public event System.Action OnJumpCapabilityLost;

    private void Start()
    {
        // Pour des tests rapides ou des triggers scénarisés
        // LoseJumpCapability(); 
    }

    /// <summary>
    /// Logique pour la perte de la capacité de saut (due au trauma à la jambe).
    /// </summary>
    public void LoseJumpCapability()
    {
        if (CanJump)
        {
            CanJump = false;
            Debug.Log("Capacité de Saut perdue : Trauma à la jambe !");
            // Déclenche l'événement pour la caméra et le feedback visuel/sonore
            OnJumpCapabilityLost?.Invoke();
            
            // Logique de feedback (visuel, sonore, etc.) ici
        }
    }
    
    // Méthode pour un retour de capacité (si nécessaire)
    public void RegainJumpCapability()
    {
        CanJump = true;
    }
}