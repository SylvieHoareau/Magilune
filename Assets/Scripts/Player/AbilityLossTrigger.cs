using UnityEngine;

public class AbilityLossTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            // Essayer de récupérer le gestionnaire de capacités sur le joueur
            PlayerAbilityManager abilityManager = other.GetComponent<PlayerAbilityManager>();

            if (abilityManager != null)
            {
                // Perte de la capacité de saut
                abilityManager.LoseJumpCapability();

                hasTriggered = true; // S'assurer que ça ne se déclenche qu'une fois

                // Ajouter un son de "ouïe !" doux et un bruit de 'boîte' qui tombe
                            
                // Optionnel: Désactiver le GameObject Trigger après utilisation
                gameObject.SetActive(false);

                Debug.Log("AbilityLossTrigger : Saut perdu → JetPack activé !");
                
            }
        }
    }
}