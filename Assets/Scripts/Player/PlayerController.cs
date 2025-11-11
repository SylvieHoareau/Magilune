using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

/// <summary>
/// Gère les mouvements, sauts, attaques et capacités du joueur.
/// Ne dépend pas directement du Input System : les entrées passent par PlayerInputHandler.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
{
    // --- Références principales ---
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInputHandler input;

    [Header("Mouvement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 10f;
    private Vector2 moveInput;

    [Header("Saut amélioré")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Capacités")]
    [SerializeField] private PlayerAbilityManager abilityManager;
    [SerializeField] private JumpAbility jumpAbility;
    [SerializeField] private JetpackAbility jetpackAbility;
    [SerializeField] private GrappleAbility grappleAbility;
    [SerializeField] private ClimbAbility climbAbility;

    [Header("Caméra Cinemachine")]
    [SerializeField] private CinemachineCamera followCamera;
    private const float BaseOrthoSize = 5f;
    private const float InjuryOrthoSize = 4.5f;

    [Header("Tir / Attaque")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f;

    private static readonly int AttackTrigger = Animator.StringToHash("AttackTrigger");

    // --- Cycle de vie Unity ---
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        input = GetComponent<PlayerInputHandler>();

        if (!rb) Debug.LogError("Rigidbody2D manquant sur PlayerController !");
        if (!animator) Debug.LogError("Animator manquant sur PlayerController !");
        if (!input) Debug.LogError("PlayerInputHandler manquant sur PlayerController !");
        if (!abilityManager) Debug.LogWarning("AbilityManager non assigné au PlayerController.");

        if (jumpAbility != null)
            jumpAbility.Initialize(rb, animator);
    }

    private void Update()
    {
        moveInput = input.MoveInput;

        // --- Gestion des entrées ---
        HandleInputActions();

        // --- Animation / visuel ---
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        MovePlayer();
        UpdatePhysicsState();
    }

    // --- Gestion des entrées ---
    private void HandleInputActions()
    {
        bool isJumpHeld = input.IsJumpHeld;

        // Jump (tap)
        if (input.JumpPressed)
            abilityManager?.HandleJumpInput(true);

        // Jetpack (maintenir)
        abilityManager?.HandleJetpackInput(isJumpHeld);

        // Grapple (tap)
        if (input.GrapplePressed)
            abilityManager?.HandleGrappleInput(true);

        // Escalade (vectorielle)
        abilityManager?.HandleClimbInput(moveInput);

        // Attaque / tir
        if (input.AttackPressed)
            animator.SetTrigger(AttackTrigger);

        if (input.ShootPressed)
            Shoot();

        if (input.InteractPressed)
            Debug.Log("Interaction détectée (futur système).");
    }

    // --- Déplacement horizontal ---
    private void MovePlayer()
    {
        float targetVelocityX = moveInput.x * speed;
        float smoothedX = Mathf.Lerp(rb.linearVelocity.x, targetVelocityX, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(smoothedX, rb.linearVelocity.y);
    }

    // --- Animation ---
    private void UpdateAnimation()
    {
        animator.SetFloat("MoveSpeed", Mathf.Abs(rb.linearVelocity.x));

        // Flip du sprite selon la direction
        if (moveInput.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1f, 1f);

        // États selon capacités
        animator.SetBool("IsGrounded", jumpAbility && jumpAbility.IsGrounded());
        animator.SetBool("IsFlying", jetpackAbility && jetpackAbility.IsUsingJetpack);
    }

    // --- Tir ---
    private void Shoot()
    {
        if (!bulletPrefab || !firePoint) return;

        var bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        if (bullet.TryGetComponent<Rigidbody2D>(out var rbBullet))
        {
            rbBullet.linearVelocity = transform.localScale.x * Vector2.right * bulletSpeed;
        }
    }

    // --- Gestion de la physique (gravité et états spéciaux) ---
    private void UpdatePhysicsState()
    {
        if (!abilityManager || !jumpAbility) return;

        // Saut amélioré
        if (!jumpAbility.IsGrounded())
        {
            if (rb.linearVelocity.y < 0)
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            else if (rb.linearVelocity.y > 0 && !input.IsJumpHeld)
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        // Gravité selon état
        if (climbAbility && climbAbility.IsClimbing())
            rb.gravityScale = 0;
        else if (grappleAbility && grappleAbility.IsGrappling())
            rb.gravityScale = 0.5f;
        else if (jetpackAbility && jetpackAbility.IsUsingJetpack)
            rb.gravityScale = 0;
        else
            rb.gravityScale = 3f;
    }

    // --- Zoom caméra (effet visuel) ---
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
