using UnityEngine;

/// <summary>
/// Gère le mouvement et la détection d'impact du projectile de grappin.
/// Ce script est placé sur le Prefab du grappin.
/// </summary>
public class GrappleProjectile : MonoBehaviour
{
    [Header("Paramètres")]
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] private float maxTravelDistance = 20f; // Distance max avant de se détruire
    [SerializeField] private LayerMask grappableLayer; // Layer des surfaces d'accroche (ex: 'Grappable')

    private Vector2 targetPosition;
    private Vector2 startPosition;
    private GrappleAbility playerGrappleAbility; // Référence au script du joueur

    /// <summary>
    /// Initialise le projectile avec les informations nécessaires.
    /// Appelé immédiatement après l'instanciation par le script du joueur.
    /// </summary>
    /// <param name="targetDir">Direction vers laquelle tirer.</param>
    /// <param name="playerAbility">Référence au script du joueur qui a tiré.</param>
    public void Initialize(Vector2 targetDir, GrappleAbility playerAbility)
    {
        // On utilise la direction pour définir la vélocité et l'angle
        GetComponent<Rigidbody2D>().linearVelocity = targetDir * projectileSpeed;
        
        // Optionnel : Faire pivoter le projectile pour pointer vers l'avant
        float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        playerGrappleAbility = playerAbility;
        startPosition = transform.position;

        // Détruire après un temps basé sur la distance max si rien n'est touché (sécurité)
        // distance / vitesse = temps
        float maxLifetime = maxTravelDistance / projectileSpeed;
        Destroy(gameObject, maxLifetime);
    }


    private void Update()
    {
        // Optionnel : Vérification de la distance pour détruire l'objet si trop loin
        if (Vector2.Distance(startPosition, transform.position) > maxTravelDistance)
        {
            // Indiquer au joueur qu'il n'y a pas eu d'accroche
            playerGrappleAbility?.OnGrappleMiss();
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// Détecte la collision avec un objet.
    /// (Le Rigidbody2D et le Collider2D sur le prefab sont requis).
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Vérifier le Layer d'accroche
        if (((1 << collision.gameObject.layer) & grappableLayer) != 0)
        {
            // ACCROCHAGE RÉUSSI

            // Calculer le point d'impact exact. Pour plus de précision, on peut utiliser Raycast.
            // Mais pour une collision simple, la position actuelle suffit :
            Vector2 hitPoint = transform.position;

            // Signaler l'accroche au joueur (avec la position de l'ancre)
            if (playerGrappleAbility != null)
            {
                playerGrappleAbility.OnGrappleHit();
            }

            // Empêcher le projectile de continuer à bouger
            if (GetComponent<Rigidbody2D>() != null)
            {
                GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            }

            // Détruire le *script* du projectile mais pas l'objet tout de suite.
            // L'objet (l'ancre visuelle) doit rester en place jusqu'à la fin du grappin.
            this.enabled = false;
        }
        else if (!collision.CompareTag("Player")) // Si on touche autre chose que le joueur (ex: un mur non grappable)
        {
            // Accrochage manqué. On détruit immédiatement.
            playerGrappleAbility?.OnGrappleMiss();
            Destroy(gameObject);
        }
    }
}