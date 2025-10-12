using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrappleAbility : MonoBehaviour
{
    // --- Références ---
    private Rigidbody2D rb;
    private LineRenderer lr;
    private PlayerAbilityManager abilityManager;
    
    // --- Paramètres de liane (Grappling) ---
    [Header("Grapple Settings")]
    [SerializeField] private float maxGrappleDistance = 15f;
    [SerializeField] private float grappleSpeed = 15f; // Vitesse de lancement/rattachement
    [SerializeField] private LayerMask grappleLayer; // Couche des objets où on peut s'accrocher
    [SerializeField] private float swingForce = 15f; // Force de balancement (pour le mouvement)

    // --- État Interne ---
    private SpringJoint2D springJoint;
    private Vector2 grapplePoint;
    private bool isGrappling = false;
    private bool grappleActive = false; // L'état d'activité pour l'AbilityManager

    // --- Initialisation ---
    public void Initialize(Rigidbody2D rbRef, PlayerAbilityManager abManagerRef)
    {
        rb = rbRef;
        abilityManager = abManagerRef;
        lr = GetComponent<LineRenderer>();
        if (lr != null) lr.enabled = false;
    }

    // --- Gérer l'Entrée du Joueur (Appelée par PlayerAbilityManager) ---
    public void HandleGrappleInput(bool isPressed)
    {
        if (!grappleActive) return;

        if (isPressed)
        {
            StartGrapple();
        }
        else if (isGrappling)
        {
            StopGrapple();
        }
    }

    // --- LOGIQUE DE GRAPPLE ---
    
    private void StartGrapple()
    {
        // Détection de Cible
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPosition - (Vector2)transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxGrappleDistance, grappleLayer);

        if (hit.collider != null)
        {
            grapplePoint = hit.point;
            isGrappling = true;
            lr.enabled = true;
            lr.positionCount = 2;

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
            lr.enabled = false;
            Debug.Log("Grapple manqué.");
        }
    }

    public void StopGrapple()
    {
        isGrappling = false;
        lr.enabled = false;
        
        // Supprimer le joint pour reprendre le contrôle total
        if (springJoint != null)
        {
            Destroy(springJoint);
        }
        
        // Réactiver la gravité normale dans PlayerController
    }
    
    // --- Gérer le Balancement (Mouvement Latéral) ---
    private void FixedUpdate()
    {
        if (isGrappling && rb != null)
        {
            // Dessiner la liane
            DrawGrappleLine();

            // Balancement : utiliser la force de mouvement du joueur pour pousser le balancier
            float horizontalInput = Input.GetAxis("Horizontal"); // Récupérer l'input de mouvement
            rb.AddForce(new Vector2(horizontalInput, 0) * swingForce, ForceMode2D.Force);
        }
    }

    // Dessiner la liane (dans LateUpdate pour un meilleur rendu)
    private void DrawGrappleLine()
    {
        if (lr.enabled)
        {
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, grapplePoint);
        }
    }

    // --- Gestion de l'état externe (PlayerAbilityManager) ---
    public void SetEnabled(bool state)
    {
        grappleActive = state;
        if (!state && isGrappling)
        {
            StopGrapple();
        }
    }

    public bool IsGrappling() => isGrappling;
}