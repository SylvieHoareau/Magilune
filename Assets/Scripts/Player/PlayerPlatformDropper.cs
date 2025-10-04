using UnityEngine;

public class PlayerPlatformDropper : MonoBehaviour
{
    // Référence au Collider2D du joueur
    [SerializeField]
    private Collider2D playerCollider;

    // Référence au Rigidbody du joueur
    private Rigidbody2D playerRigidbody;

    // LayerMask pour les plateformes traversables
    [SerializeField]
    private LayerMask passThroughPlatformLayer;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        // Si le Collider n'est pas assigné dans l'inspecteur, on essaie de le récupérer automatiquement
        if (playerCollider == null)
        {
            playerCollider = GetComponent<Collider2D>();
        }
    }

    private void TryDropThroughPlatform()
    {
        // Vérifie si le joueur appuie sur la touche de descente (par exemple, la touche S ou la flèche bas)
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            // Vérifie si le joueur est au-dessus d'une plateforme traversable
            RaycastHit2D hit = Physics2D.Raycast(playerCollider.bounds.center, Vector2.down, playerCollider.bounds.extents.y + 0.1f, passThroughPlatformLayer);
            if (hit.collider != null)
            {
                Collider2D platformCollider = hit.collider;
                Physics2D.IgnoreCollision(playerCollider, platformCollider, true);

                // Désactive temporairement le collider du joueur pour permettre de traverser la plateforme
                StartCoroutine(DisableColliderTemporarily(platformCollider, delay: 0.2f));
            }
        }
    }

    // Coroutine pour réactiver la collision après un court délai
    private System.Collections.IEnumerator DisableColliderTemporarily(Collider2D collider, float delay)
    {
        // Désactive le collider du joueur
        playerCollider.enabled = false;

        // Attend un court instant pour permettre au joueur de traverser la plateforme
        yield return new WaitForSeconds(0.2f);

        // Réactive le collider du joueur
        playerCollider.enabled = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
