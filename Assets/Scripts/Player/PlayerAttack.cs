using UnityEngine;
using UnityEngine.InputSystem; 
// N'oubliez pas cet using pour l'Input System

// Correction de l'interface à implémenter : PlayerControls.IPlayerActions
public class PlayerAttack : MonoBehaviour, PlayerControls.IPlayerActions 
{
    [Header("Dependencies")]
    [SerializeField] private Animator _animator;

    private PlayerControls _playerControls; // Référence à l'Input Actions Asset
    private static readonly int AttackTriggerHash = Animator.StringToHash("AttackTrigger");

    // S'assurer que les dépendances sont là
    private void Awake()
    {
        if (_animator == null)
        {
            Debug.LogError("Animator manquant sur PlayerAttack.");
        }
        
        // Initialisation et association des callbacks
        _playerControls = new PlayerControls();
        _playerControls.Player.SetCallbacks(this); // 'Player' est le nom de votre Action Map
    }
    
    // Activer le contrôle lorsque l'objet devient actif
    private void OnEnable()
    {
        _playerControls.Player.Enable();
    }
    
    // Désactiver le contrôle lorsque l'objet est désactivé
    private void OnDisable()
    {
        _playerControls.Player.Disable();
    }
    
    // L'input est maintenant géré par la méthode OnAttack ci-dessous.

    // ✅ La méthode de Callback pour l'Action "Attack"
    public void OnAttack(InputAction.CallbackContext context)
    {
        // On vérifie que l'action a été complétée (bouton pressé)
        if (context.performed)
        {
            // Vérifier si le joueur n'est pas déjà en train d'attaquer
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !_animator.IsInTransition(0))
            {
                // Déclenche la transition dans l'Animator Controller
                _animator.SetTrigger(AttackTriggerHash);
            }
        }
    }
    
    // Vous devez également implémenter les autres méthodes de l'interface IPlayerActions, 
    // même si elles sont vides pour l'instant. (Exemples basés sur un Platformer standard)
    public void OnMove(InputAction.CallbackContext context) { }
    public void OnJump(InputAction.CallbackContext context) { }
    public void OnLook(InputAction.CallbackContext context) { }
}