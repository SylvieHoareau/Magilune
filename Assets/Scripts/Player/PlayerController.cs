using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections; // pour afficher les coroutines

public class PlayerController : MonoBehaviour, PlayerControls.IPlayerActions
{
    private Rigidbody2D rb;
    // Changement de PlayerActions à GameInput (ou le nouveau nom que vous avez choisi)
    private PlayerControls playerControls; // Utiliser @GameInput si vous avez coché "Generate C# Class"
    private Animator _animator;

    // Ajout d'une référence à l'Animator (pour l'attaque) et au Rigidbody (pour le mouvement)
    [Header("Mouvement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 10f;

    private Vector2 moveInput;
    private static readonly int AttackTriggerHash = Animator.StringToHash("AttackTrigger");

    [Header("Modules de Capacité")]
    [SerializeField] private PlayerAbilityManager abilityManager;
    [SerializeField] private JumpAbility jumpAbility;
    [SerializeField] private JetpackAbility jetpackAbility;
    [SerializeField] private GrappleAbility grappleAbility;
    [SerializeField] private ClimbAbility climbAbility; 


    [Header("Saut Amélioré")]
    [SerializeField] private float fallMultiplier = 2.5f; // Gravité supplémentaire en chute
    [SerializeField] private float lowJumpMultiplier = 2f; // Gravité supplémentaire si saut relâché tôt

    // -- Référence à la virtual Camera pour le zoom/dézoom --
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera followCamera;
    private const float BaseOrthoSize = 5f; // Taille de base de votre caméra
    private const float InjuryOrthoSize = 4.5f; // Léger zoom pour le feedback de trauma

    [SerializeField] private LayerMask passThroughLayer;
    [SerializeField] private float disableTime = 0.5f;

    // [Header("Health")]
    // [SerializeField] private PlayerHealth playerHealth; // référence au script PlayerHealth

    [Header("Shoot")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint; // Position où la balle sort
    [SerializeField] private float bulletSpeed = 10f;

    private Collider2D playerCollider;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        if (rb == null || _animator == null)
        {
            Debug.LogError("Dependencies manquantes (Rigidbody2D ou Animator) sur PlayerController.");
        }

        // Création de l'instance avec le nouveau nom de classe
        playerControls = new PlayerControls();

        // Association des Callbacks à CETTE instance
        playerControls.Player.SetCallbacks(this);

        // Initialisation des modules de capacité
        jumpAbility.Initialize(rb, _animator); 

        // Vérification des dépendances
        if (abilityManager == null || jumpAbility == null)
        {
            Debug.LogError("PlayerController : Références AbilityManager/JumpAbility manquantes.");
        }

        // S'abonner à l'événement de perte de capacité (FEEDBACK DE CAMERA)
        abilityManager.OnJumpCapabilityLost += HandleJumpLossFeedback;

        // if (playerHealth == null)
        // {
        //     playerHealth = GetComponent<PlayerHealth>();
        //     if (playerHealth == null)
        //     {
        //         Debug.LogError("PlayerController : Référence PlayerHealth manquante.");
        //     }
        // }
    }

    void Start()
    {
        playerCollider = GetComponent<Collider2D>();

        // Initialisation du Grapple
        if (abilityManager != null && abilityManager.TryGetComponent<GrappleAbility>(out GrappleAbility grapple))
        {
            grappleAbility = grapple;
            grappleAbility.Initialize(rb, abilityManager); // On passe le Rigidbody et l'AbilityManager
        }

        // Initialisation du ClimbAbility
        if (abilityManager != null && abilityManager.TryGetComponent<ClimbAbility>(out ClimbAbility climb))
        {
            climbAbility = climb;
            climbAbility.Initialize(rb, _animator);
        }

        // S'abonner à l'événement de perte de capacité (pour les zooms/feedbacks)
        abilityManager.OnJumpCapabilityLost += HandleJumpLossFeedback;
    }

    void OnEnable()
    {
        playerControls.Player.Enable();    
    }

    void OnDisable()
    {
        playerControls.Player.Disable();    
    }

    // ----------------------------------------
    // Implémentation de l'interface IPlayerActions
    // ----------------------------------------
    // Gestion du Mouvement (Utilisé pour l'input Vector2)
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log("Move input reçu : " + moveInput);
        
        // Transmettre l'input au gestionnaire d'escalade
        abilityManager.HandleClimbInput(moveInput);
    }

    // Gestion de l'Attaque (Input de type Bouton)
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // La logique d'attaque est réintégrée ici
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !_animator.IsInTransition(0))
            {
                _animator.SetTrigger(AttackTriggerHash);
            }
        }
    }

    // Méthode pour gérer la gravité améliorée (chute rapide et saut court)
    private void UpdatePhysicsState()
    {
        // C'est le gestionnaire qui décide de la gravité.
        if (abilityManager.CanJump)
        {
            // PHASE 1 : Saut normal. On utilise la gravité d'Unity (par défaut à 1)
            if (rb.gravityScale != 1f) rb.gravityScale = 1f;
        }
        else if (abilityManager.CanUseJetpack)
        {
            // PHASE 2 : Jetpack. On force la gravité à 0 (ou une petite valeur) 
            // car le jetpack applique sa propre force.
            if (rb.gravityScale != 0f) rb.gravityScale = 0f;

            // Mise à jour de l'état du jetpack dans FixedUpdate
            abilityManager.HandleJetpackInput(IsJumpInputHeld); // On utilise l'état de l'input
        }
    }

    // Stocker l'état de l'input du saut
    private bool IsJumpInputHeld = false;

    // Implémentation des autres actions (Doivent être présentes)
    // ----------------------------------------
    // Logique de Saut dans le Controller (après refactoring)
    // ----------------------------------------
    public void OnJump(InputAction.CallbackContext context)
    {
        // On met à jour l'état de l'input de saut/jetpack
        IsJumpInputHeld = context.ReadValue<float>() > 0;

        // GESTION DU SAUT (Phase 1)
        if (abilityManager.CanJump)
        {
            if (context.performed)
            {
                jumpAbility.PerformJump();
                // Si JumpAbility délègue l'animation, vous n'avez rien à faire ici. 
                // Sinon, appelez-le ici :
                // _animator.SetBool("IsJumping", true); 
            }
        }

        // GESTION DU JETPACK (Phase 2 ou co-active)
        // La gestion du jetpack doit se faire dans Update/FixedUpdate en lisant l'input,
        // ou ici si c'est un toggle (ce qui est rare pour un jetpack)
        bool isPressed = context.ReadValue<float>() > 0; // Vrai si le bouton est enfoncé
        abilityManager.HandleJetpackInput(isPressed);
    }
    
    public void OnGrapple(InputAction.CallbackContext context)
    {
        if (abilityManager == null) return;

        if (context.performed)
        {
            // On délègue l'exécution pure de l'action
            grappleAbility.StartGrapple();
            abilityManager.HandleGrappleInput(true); // Commencer le grapple
        }
        else if (context.canceled)
        {
            abilityManager.HandleGrappleInput(false); // Arrêter le grapple
        }
    }
    public void OnLook(InputAction.CallbackContext context) { }

    /// <summary>
    /// Gère l'action d'Interaction/Locomotion (Grappin/Jetpack/Grimpe)
    /// </summary>
    public void OnInteract(InputAction.CallbackContext context)
    {
        // La spécification demande "Interact (Locomotion system in/out)"
        if (context.performed)
        {
            // PROCHAINES ÉTAPES : Appeler un AbilityManager ou un LocomotionSystem ici.
            // abilityManager.ToggleLocomotion();
            Debug.Log("Interact Input Reçu : Préparation du système de Locomotion.");
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
        if (rbBullet != null)
        {
            rbBullet.linearVelocity = transform.localScale.x * Vector2.right * bulletSpeed;
        }
    }

    // Pour changer de caméra (playerFollow/Panoramic)
    public void OnSwitchCamera(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Appui sur SwitchCamera détecté !");
            // Ici, tu peux appeler ton CameraSwitcher :
            FindFirstObjectByType<CameraSwitcher>()?.SendMessage("OnSwitchCamera");
        }
    }

    // ----------------------------------------
    // LOGIQUE DE JEU
    // -------------------------------------------

    void FixedUpdate()
    {
        // Gérer la gravité et les capacités avant le mouvement
        UpdatePhysicsState();
        
        // Calcul de la vélocité cible en fonction de l'input
        float targetXVelocity = moveInput.x * speed;

        // Application de l'accélération/lissage (Lerp) UNIQUEMENT sur l'axe X.
        // On conserve la vélocité Y actuelle (gravité, saut, jetpack)
        float newXVelocity = Mathf.Lerp(rb.linearVelocity.x, targetXVelocity, acceleration * Time.fixedDeltaTime);

        // Application de la nouvelle vélocité au Rigidbody2D
        // Si la gravityScale est 0 (pour le jetpack), c'est au jetpack/saut de définir rb.linearVelocity.y
        // Si la gravityScale est > 0, rb.linearVelocity.y est gérée par Unity Physics
        rb.linearVelocity = new Vector2(newXVelocity, rb.linearVelocity.y);

        // GESTION DE LA PRIORITE DES CAPACITES
        bool isAnyAbilityActive = false;

        // Mouvement : mettre à jour MoveSpeed pour Run/Idle
        _animator.SetFloat("MoveSpeed", Mathf.Abs(rb.linearVelocity.x));

        // Mettre à jour l'état de saut (IsJumping, Atterrissage, Chute)
        if (jumpAbility.IsEnabled) // Si la capacité de saut est encore active (Phase 1)
        {
            jumpAbility.UpdateAnimationState();
        }

        // Si le joueur est en l'air (JumpAbility.IsGrounded() == false)
        if (!jumpAbility.IsGrounded())
        {
            // Chute rapide (quand la vélocité est négative, i.e., après le sommet)
            if (rb.linearVelocity.y < 0)
            {
                // Appliquer plus de force de gravité pour une chute rapide
                // (La formule ajoute de la force pour que la chute soit plus raide)
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }

            // Saut court (si le joueur relâche la touche de saut)
            // La variable 'jumpInputHeld' doit être mise à jour dans la méthode OnJump ou OnAction du nouveau système Input
            // Supposons que PlayerController a une booléenne `isJumpInputHeld`
            else if (rb.linearVelocity.y > 0 /* && !isJumpInputHeld (si vous avez implémenté ça) */)
            {
                // Appliquer plus de force pour couper l'ascension
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
        }

        // Escalade (Priorité maximale)
        if (climbAbility != null && climbAbility.IsClimbing())
        {
            // La logique d'escalade et de physique (rb.linearVelocity = ...) est dans ClimbAbility.HandleClimbInput()
            isAnyAbilityActive = true;
            return; // Sortir immédiatement pour ignorer le mouvement horizontal normal et la gravité
        }

        // Grappling
        if (grappleAbility != null && grappleAbility.IsGrappling())
        {
            // Réduire la gravité ou la neutraliser pour un meilleur balancement
            rb.gravityScale = 0.5f; // ou 0f si vous voulez un balancement parfait

            // Le balancement (AddForce) se fait dans GrappleAbility.FixedUpdate()

            // S'il est en Grappling, on peut sortir et ignorer le saut/gravité avancée
            return;
        }
        
        // Jetpack
        if (abilityManager.CanUseJetpack && jetpackAbility != null && jetpackAbility.IsUsingJetpack)
        {
            // La logique de force du Jetpack est dans JetpackAbility.HandleJetPack()
            isAnyAbilityActive = true;
            // Ne pas faire de 'return' ici si vous voulez que le joueur puisse se déplacer horizontalement en Jetpack.
        }

         // --- LOGIQUE DE GRAVITÉ AMÉLIORÉE (Si aucune capacité spéciale n'est active) ---

        if (!isAnyAbilityActive)
        {
            // Réinitialiser la gravité au cas où une autre capacité l'aurait changée
            rb.gravityScale = 3f; // Assurez-vous que c'est votre gravité de base
        }
    }

    void Update()
    {
        // Animation de movement
        // Utiliser Mathf.Abs(rb.linearVelocity.x) est plus précis pour l'animation de marche/course.
        _animator.SetFloat("MoveSpeed", Mathf.Abs(rb.linearVelocity.x));

        // Flip visuel
        if (moveInput.x != 0)
        {
            // Inverse le scale X pour retourner le sprite horizontalement
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1f, 1f);
        }

        // Gestion du saut
        bool isGrounded = jumpAbility.IsGrounded();
        _animator.SetBool("IsGrounded", isGrounded);

        // Gestion du jetpack
        _animator.SetBool("IsFlying", jetpackAbility != null && jetpackAbility.IsUsingJetpack);

        if (Input.GetKeyDown(KeyCode.S)) // ou KeyCode.DownArrow
        {
            StartCoroutine(DisableCollision());
        }

        // Gestion de la mort du Player
        // if (playerHealth.IsDead)
        // {
        //     // Désactive les contrôles
        //     _playerControls.Disable();
        //     // Autres logiques de mort (animation, son, etc.)
        //     _animator.SetTrigger("Die");
        //     rb.linearVelocity = Vector2.zero; // Arrête le mouvement
        //     this.enabled = false; // Désactive ce script
        //     // return;
        // }
    }

    // ----------------------------------------
    // Gestion du Feedback de Perte de Capacité (Caméra)
    // ----------------------------------------
    private void HandleJumpLossFeedback()
    {
        // 1. Zoom de la caméra (pour mettre l'accent sur le joueur)
        if (followCamera != null)
        {
            StartCoroutine(SmoothZoom(InjuryOrthoSize, 0.5f));
        }

        // 2. Autre feedback (son, particules, etc.)
    }

    /// <summary>
    /// Coroutine pour lisser la transition de taille orthographique de Cinemachine.
    /// </summary>
    private IEnumerator SmoothZoom(float targetSize, float duration)
    {
        float startSize = followCamera.Lens.OrthographicSize;
        float time = 0;

        while (time < duration)
        {
            followCamera.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        followCamera.Lens.OrthographicSize = targetSize;
    }

    private IEnumerator DisableCollision()
    {
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("PassThrough"), true);
        yield return new WaitForSeconds(disableTime);
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("PassThrough"), false);
    }

    // ------------------------------------
    // Gestion du Débogage (IPlayerActions)
    // ------------------------------------

    // Ces fonctions reçoivent les entrées de débogage et INVERSENT l'état de la capacité.

    public void OnDebugJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // On demande au Manager d'inverser l'état actuel de CanJump
            abilityManager.SetJumpCapability(!abilityManager.CanJump);

        }
    }

    public void OnDebugJetpack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // On demande au Manager d'inverser l'état actuel de CanUseJetpack
            abilityManager.SetJetpackCapability(!abilityManager.CanUseJetpack);
        }
    }

    public void OnDebugClimb(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // On demande au Manager d'inverser l'état actuel de CanClimb
            abilityManager.SetClimbCapability(!abilityManager.CanClimb);
        }
    }

    public void OnDebugGrappling(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // On demande au Manager d'inverser l'état actuel de CanGrapple
            abilityManager.SetGrappleCapability(!abilityManager.CanGrapple);
        }    
    }
}