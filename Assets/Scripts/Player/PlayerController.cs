using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour, PlayerControls.IPlayerActions
{
    private Rigidbody2D rb;
    // Changement de PlayerActions à GameInput (ou le nouveau nom que vous avez choisi)
    private PlayerControls _playerControls; // Utiliser @GameInput si vous avez coché "Generate C# Class"
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

    // -- Référence à la virtual Camera pour le zoom/dézoom --
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera followCamera;
    private const float BaseOrthoSize = 5f; // Taille de base de votre caméra
    private const float InjuryOrthoSize = 4.5f; // Léger zoom pour le feedback de trauma
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        if (rb == null || _animator == null)
        {
            Debug.LogError("Dependencies manquantes (Rigidbody2D ou Animator) sur PlayerController.");
        }

        rb.gravityScale = 0;

        // Création de l'instance avec le nouveau nom de classe
        _playerControls = new PlayerControls();


        // Association des Callbacks à CETTE instance
        _playerControls.Player.SetCallbacks(this);

        // Vérification des dépendances
        if (abilityManager == null || jumpAbility == null)
        {
            Debug.LogError("PlayerController : Références AbilityManager/JumpAbility manquantes.");
        }

        // S'abonner à l'événement de perte de capacité (FEEDBACK DE CAMERA)
        abilityManager.OnJumpCapabilityLost += HandleJumpLossFeedback;

    }

    void OnEnable()
    {
        _playerControls.Enable(); // Pro-tip: Activer dans OnEnable
    }

    void OnDisable()
    {
        _playerControls.Disable(); // Pro-tip: Désactiver dans OnDisable pour la gestion de l'état
    }

    // ----------------------------------------
    // Implémentation de l'interface IPlayerActions
    // ----------------------------------------
    // Gestion du Mouvement (Utilisé pour l'input Vector2)
    public void OnMove(InputAction.CallbackContext context)
    {
        // Votre logique originale de LinkActions() est déplacée ici, simplifiée.
        moveInput = context.ReadValue<Vector2>();
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

    // Implémentation des autres actions (Doivent être présentes)
    // ----------------------------------------
    // Logique de Saut dans le Controller (après refactoring)
    // ----------------------------------------
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Le Controller vérifie si la capacité est ACTUELLEMENT active
            if (abilityManager.CanJump)
            {
                // Le Controller délègue l'exécution de la logique de saut au module
                jumpAbility.PerformJump();
            }
            // Pro-Tip: Si la capacité est perdue, vous pouvez jouer un son "bruit de jambe cassée" ou un feedback ici.
        }
    }
    public void OnLook(InputAction.CallbackContext context) { }

    // ----------------------------------------
    // LOGIQUE DE JEU
    // -------------------------------------------

    void FixedUpdate()
    {
        // Le mouvement physique doit être dans FixedUpdate()
        Vector2 targetVelocity = moveInput * speed;

        // Application d'une accélération progressive (mouvement cinématique)
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
    }

    void Update()
    {
        // Animation dans Update()
        _animator.SetFloat("MoveSpeed", rb.linearVelocity.magnitude);
        // L'Animator utilise la vélocité réelle, pas seulement l'input brut

        // Mettre à jour la direction pour le Blend Tree
        // Vous devez aussi gérer le Flip du SpriteRenderer ici pour la direction visuelle
        if (moveInput.x != 0)
        {
            // Inverse le scale X pour retourner le sprite horizontalement
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1f, 1f);
        }
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
    private System.Collections.IEnumerator SmoothZoom(float targetSize, float duration)
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
}