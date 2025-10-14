using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrappleAbility : MonoBehaviour
{

    // --- État Interne de la Liane ---
    private enum GrappleState { Idle, Flying, Grappling } // Nouveaux états
    private GrappleState currentState = GrappleState.Idle;

    // --- Références ---
    private Rigidbody2D rb;
    private LineRenderer lr;
    private SpringJoint2D springJoint; // Le composant de ressort qui crée le balancier
    
    // --- Paramètres de liane (Grappling) à ajuster dans l'éditeur ---
    [Header("Grapple Settings")]
    [SerializeField] private float maxGrappleDistance = 15f;
    [SerializeField] private float grappleSpeed = 15f; // Vitesse de lancement/rattachement
    [SerializeField] private LayerMask grappleLayer; // Couche des objets où on peut s'accrocher
    [SerializeField] private float swingForce = 15f; // Force de balancement (pour le mouvement)

    // --- Variables de l'Action ---
    private Vector2 grapplePoint;
    // Position intermédiaire pour l'animation de vol
    private Vector2 currentGrapplePosition;
    private bool isGrappleEnabled = true;

    // --- Initialisation ---
    public void Initialize(Rigidbody2D rbRef, PlayerAbilityManager abManagerRef)
    {
        rb = rbRef;
        lr = GetComponent<LineRenderer>();
        if (lr != null) lr.enabled = false;
    }

    // --- Gérer l'Entrée du Joueur (Appelée par PlayerAbilityManager) ---
    public void HandleGrappleInput(bool isPressed)
    {
        if (!isGrappleEnabled) return;

        if (isPressed)
        {
            StartGrapple();
        }
        else
        {
            StopGrapple();
        }
    }

    // --- LOGIQUE DE DEMARRAGE ---
    
    private void StartGrapple()
    {
        // Déterminer où le joueur vise
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPosition - (Vector2)transform.position).normalized;

        // Lancer un rayon (un tir invisible) pour trouver une cible
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxGrappleDistance, grappleLayer);

        if (hit.collider != null)
        {
            // Cible Trouvée
            grapplePoint = hit.point;
            currentGrapplePosition = transform.position; // Le crochet démarre de nous
            currentState = GrappleState.Flying;  // Passer en mode "vol"          
            
            lr.enabled = true;
            lr.positionCount = 2;

            Debug.Log($"Grapple lancé vers {hit.collider.name}");

            // Création du Joint de Ressort (Liane)
            springJoint = gameObject.AddComponent<SpringJoint2D>();
            springJoint.autoConfigureDistance = false;
            springJoint.distance = Vector2.Distance(transform.position, grapplePoint);
            springJoint.dampingRatio = 1f; // Peu d'amortissement pour un bon balancier
            springJoint.frequency = 5f; // La rigidité du 'ressort'
            springJoint.connectedAnchor = grapplePoint;

            // Désactiver la gravité temporairement pour le balancement (ou la réduire)
            // On le fait dans PlayerController pour éviter les conflits
            
            // Mettre à jour la caméra (facultatif : pour un effet visuel)
            // Lier le SpringJoint à un objet vide si on veut un mouvement plus précis

            Debug.Log($"Grapple lancé vers {grapplePoint} sur {hit.collider.name}");
        }
        else
        {
            currentState = GrappleState.Idle; // Rester inactif
            lr.enabled = false;
            Debug.Log("Grapple manqué. Aucune surface accrochable trouvée.");
        }
    }

    public void StopGrapple()
    {
        if (currentState == GrappleState.Idle) return; // Déjà arrêté

        currentState = GrappleState.Idle; // Arrêt
        lr.enabled = false;

        // Supprimer le joint pour reprendre le contrôle total
        if (springJoint != null)
        {
            Destroy(springJoint);
            springJoint = null;
        }

        // Réactiver la gravité normale dans PlayerController
        Debug.Log("Grapple arrêté.");
    }

    // Gère la suite après la détection d'une cible par Raycast
    public void OnGrappleHit()
    {
        // Mise à jour de l'état
        currentGrapplePosition = transform.position;
        currentState = GrappleState.Flying; // On commence par l'état de vol du crochet

        // Préparation visuelle
        lr.enabled = true;
        lr.positionCount = 2;

        // Le SpringJoint est créé *seulement* dans AttachGrapple() après que le crochet ait atteint sa cible !
    }

    // Gère l'échec du Raycast
    public void OnGrappleMiss()
    {
        // Nettoyage de l'état
        currentState = GrappleState.Idle; 
        lr.enabled = false;
        
        // Logique d'échec
        // (TODO : Jouer un son d'échec ici)
    }

    
    // Mise à jour du vol et de l'attachement
    private void Update()
    {
        if (currentState == GrappleState.Flying)
        {
            // Déplacer la position du "crochet" vers le point d'accroche
            currentGrapplePosition = Vector2.MoveTowards(currentGrapplePosition, grapplePoint, grappleSpeed * Time.deltaTime);

            // Si le crochet a atteint la cible
            if (Vector2.Distance(currentGrapplePosition, grapplePoint) < 0.1f)
            {
                AttachGrapple(); // Créer le SpringJoint
            }
        }
        // Assurez-vous que isGrappling dans PlayerController est mis à jour
        // abilityManager.SetGrapplingState(currentState == GrappleState.Grappling);
    }

    // Action d'attachement après le vol du crochet 
    private void AttachGrapple()
    {
        // Changer l'état
        currentState = GrappleState.Grappling; 

        // Création du Joint de Ressort (Liane) - **Maintenant seulement**
        springJoint = gameObject.AddComponent<SpringJoint2D>();
        springJoint.autoConfigureDistance = false;
        
        // La distance initiale est celle de l'impact
        springJoint.distance = Vector2.Distance(transform.position, grapplePoint);
        springJoint.dampingRatio = 1f;
        springJoint.frequency = 5f;
        springJoint.connectedAnchor = grapplePoint;

        // Jouer un son

        Debug.Log("Grapple Accroché ! Balancement actif.");

    }

    // --- Gérer le Balancement (Mouvement Latéral) ---
    private void FixedUpdate()
    {
        // On ne gère le balancement que si on est accroché
        if (currentState == GrappleState.Flying && rb != null)
        {
            // Balancement : utiliser la force de mouvement du joueur pour pousser le balancier
            float horizontalInput = Input.GetAxis("Horizontal"); // Récupérer l'input de mouvement
            rb.AddForce(new Vector2(horizontalInput, 0) * swingForce, ForceMode2D.Force);
        }
    }
    
    // Pour un meilleur rendu visuel, dessiner la liane dans LateUpdate
    private void LateUpdate()
    {
        if (currentState == GrappleState.Flying)
        {
            DrawGrappleLine();
        }
    }

    // Dessiner la liane entre le joueur et le point d'accroche (dans LateUpdate pour un meilleur rendu)
    private void DrawGrappleLine()
    {
        // Point de départ : le joueur
        lr.SetPosition(0, transform.position);

        // Si on est accroché, le point final est la cible (grapplePoint)
        if (currentState == GrappleState.Grappling)
        {
            lr.SetPosition(1, grapplePoint);
        }
        // Si on est en vol, le point final est la position courante du crochet
        else if (currentState == GrappleState.Flying)
        {
            lr.SetPosition(1, currentGrapplePosition);
        }
    }
    


    // --- Gestion de l'état externe (PlayerAbilityManager) ---
    public void SetEnabled(bool state)
    {
        // Mettez ici toute la logique nécessaire pour désactiver ou réactiver le module.
        // Par exemple :
        this.enabled = state; 
        // ou
        // isEnabled = state;
    }
    public bool IsGrappling() => currentState == GrappleState.Grappling;

}