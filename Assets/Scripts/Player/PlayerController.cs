using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections;
using Unity.VisualScripting; // pour afficher les coroutines

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, PlayerControls.IPlayerActions
{
    // Ajout d'une référence à l'Animator (pour l'attaque) et au Rigidbody (pour le mouvement)
    private Rigidbody2D rb;
    private Animator animator;
    // private PlayerControls playerControls;

    [Header("Mouvement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 10f;
    private Vector2 moveInput;

    [Header("Saut amélioré")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    private bool isJumpHeld = false;

    [Header("Références")]
    [SerializeField] private PlayerAbilityManager abilityManager;
    [SerializeField] private JumpAbility jumpAbility;
    [SerializeField] private JetpackAbility jetpackAbility;
    [SerializeField] private GrappleAbility grappleAbility;
    [SerializeField] private ClimbAbility climbAbility;

    // -- Référence à la virtual Camera pour le zoom/dézoom --
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera followCamera;
    private const float BaseOrthoSize = 5f;
    private const float InjuryOrthoSize = 4.5f;

    [Header("Tir")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f;

    private static readonly int AttackTrigger = Animator.StringToHash("AttackTrigger");
    private Collider2D playerCollider;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb == null || animator == null)
        {
            Debug.LogError("Dependencies manquantes (Rigidbody2D ou Animator) sur PlayerController.");
        }

        // playerControls = new PlayerControls();
        // playerControls.Player.SetCallbacks(this);

        if (abilityManager == null)
            Debug.LogWarning("AbilityManager manquant sur le PlayerController.");

        if (jumpAbility != null)
            jumpAbility.Initialize(rb, animator);
    }

    // private void OnEnable() => playerControls.Player.Enable();
    // private void OnDisable() => playerControls.Player.Disable();

    // Gestion du Mouvement (Utilisé pour l'input Vector2)
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log("Move input reçu : " + moveInput);
        abilityManager?.HandleClimbInput(moveInput);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
       isJumpHeld = context.ReadValue<float>() > 0;

        if (context.performed && abilityManager != null && abilityManager.CanJump)
            jumpAbility?.PerformJump();

        // Gestion jetpack
        bool pressed = context.ReadValue<float>() > 0;
        abilityManager?.HandleJetpackInput(pressed);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animator.SetTrigger("AttackTrigger");
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
        if (!bulletPrefab || !firePoint) return;
        var bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        if (bullet.TryGetComponent<Rigidbody2D>(out var rbBullet))
        {
            rbBullet.linearVelocity = transform.localScale.x * Vector2.right * bulletSpeed;
        }
    }

    public void OnGrapple(InputAction.CallbackContext context)
    {
        if (abilityManager == null) return;

        if (context.performed)
        {
            grappleAbility?.StartGrapple();
            abilityManager.HandleGrappleInput(true);
        }
        else if (context.canceled)
        {
            abilityManager.HandleGrappleInput(false);
        }
    }

      public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
            Debug.Log("Interact input reçu — futur système de locomotion.");
    }

    public void OnLook(InputAction.CallbackContext context) { }

    public void OnSwitchCamera(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            FindFirstObjectByType<CameraSwitcher>()?.SendMessage("OnSwitchCamera");
        }
    }

    // --- Débogage ---
    public void OnDebugJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            abilityManager?.SetJumpCapability(!abilityManager.CanJump);
    }

    public void OnDebugJetpack(InputAction.CallbackContext context)
    {
        if (context.performed)
            abilityManager?.SetJetpackCapability(!abilityManager.CanUseJetpack);
    }

    public void OnDebugClimb(InputAction.CallbackContext context)
    {
        if (context.performed)
            abilityManager?.SetClimbCapability(!abilityManager.CanClimb);
    }

    public void OnDebugGrappling(InputAction.CallbackContext context)
    {
        if (context.performed)
            abilityManager?.SetGrappleCapability(!abilityManager.CanGrapple);
    }

    // --- Boucles principales ---
    private void Update()
    {
        animator.SetFloat("MoveSpeed", Mathf.Abs(rb.linearVelocity.x));

        // Flip du sprite
        if (moveInput.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1f, 1f);

        animator.SetBool("IsGrounded", jumpAbility != null && jumpAbility.IsGrounded());
        animator.SetBool("IsFlying", jetpackAbility != null && jetpackAbility.IsUsingJetpack);
    }

    private void FixedUpdate()
    {
        UpdatePhysicsState();
        float targetVelocityX = moveInput.x * speed;
        float smoothedX = Mathf.Lerp(rb.linearVelocity.x, targetVelocityX, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(smoothedX, rb.linearVelocity.y);
    }

    private void UpdatePhysicsState()
    {
        if (abilityManager == null) return;

        if (!jumpAbility.IsGrounded())
        {
            if (rb.linearVelocity.y < 0)
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            else if (rb.linearVelocity.y > 0 && !isJumpHeld)
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        if (climbAbility != null && climbAbility.IsClimbing())
        {
            rb.gravityScale = 0;
            return;
        }

        if (grappleAbility != null && grappleAbility.IsGrappling())
        {
            rb.gravityScale = 0.5f;
            return;
        }

        if (jetpackAbility != null && jetpackAbility.IsUsingJetpack)
        {
            rb.gravityScale = 0;
            return;
        }

        rb.gravityScale = 3f; // valeur par défaut
    }

    // Exemple de zoom cinématique
    private void HandleJumpLossFeedback()
    {
        if (followCamera != null)
            StartCoroutine(SmoothZoom(InjuryOrthoSize, 0.5f));
    }
    
    private IEnumerator SmoothZoom(float targetSize, float duration)
    {
        float start = followCamera.Lens.OrthographicSize;
        float t = 0;
        while (t < duration)
        {
            followCamera.Lens.OrthographicSize = Mathf.Lerp(start, targetSize, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        followCamera.Lens.OrthographicSize = targetSize;
    }

}