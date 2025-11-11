using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gère toutes les entrées du joueur.
/// Sert d’interface entre le Input System Unity et le PlayerController.
/// </summary>
[DefaultExecutionOrder(-1)] // S'assure que ce script s’exécute avant le PlayerController
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput playerInput;

    // --- Mouvements ---
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool IsJumpHeld { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool ShootPressed { get; private set; }
    public bool GrapplePressed { get; private set; }
    public bool InteractPressed { get; private set; }

    // --- Configuration ---
    [Header("Debug")]
    [SerializeField] private bool logInputs = false;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (!playerInput)
        {
            Debug.LogError("Aucun PlayerInput trouvé sur ce GameObject !");
        }
    }

    // --- Méthodes d’entrée appelées par le Input System ---
    // (ces noms doivent correspondre aux Actions de ton InputActions)

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        if (logInputs) Debug.Log($"Move: {MoveInput}");
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            JumpPressed = true;
            IsJumpHeld = true;
            if (logInputs) Debug.Log("Jump started");
        }
        else if (context.canceled)
        {
            IsJumpHeld = false;
            if (logInputs) Debug.Log("Jump released");
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            AttackPressed = true;
            if (logInputs) Debug.Log("Attack pressed");
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ShootPressed = true;
            if (logInputs) Debug.Log("Shoot pressed");
        }
    }

    public void OnGrapple(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GrapplePressed = true;
            if (logInputs) Debug.Log("Grapple pressed");
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InteractPressed = true;
            if (logInputs) Debug.Log("Interact pressed");
        }
    }

    // --- Mise à jour / réinitialisation des flags ---
    private void LateUpdate()
    {
        // Ces flags ne durent qu'une frame (simule un "pressed this frame")
        JumpPressed = false;
        AttackPressed = false;
        ShootPressed = false;
        GrapplePressed = false;
        InteractPressed = false;
    }
}
