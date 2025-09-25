using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    // Changement de PlayerActions à GameInput (ou le nouveau nom que vous avez choisi)
    private @GameInput inputActions; // Utiliser @GameInput si vous avez coché "Generate C# Class"
    private Animator animator;

    private Vector2 moveInput;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.gravityScale = 0;

        // Création de l'instance avec le nouveau nom de classe
        inputActions = new @GameInput(); 
        inputActions.Enable();

        LinkActions();
    }

    void OnEnable()
    {
        inputActions.Enable(); // Pro-tip: Activer dans OnEnable
    }

    void OnDisable()
    {
        inputActions.Disable(); // Pro-tip: Désactiver dans OnDisable pour la gestion de l'état
    }

    void LinkActions()
    {
        // Les noms des Maps et Actions sont inchangés
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>(); 
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero; 
    }

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
        animator.SetFloat("MoveSpeed", rb.linearVelocity.magnitude); 
        // L'Animator utilise la vélocité réelle, pas seulement l'input brut
        
        // Mettre à jour la direction pour le Blend Tree
        // Vous devez aussi gérer le Flip du SpriteRenderer ici pour la direction visuelle
        if (moveInput.x != 0)
        {
             // Inverse le scale X pour retourner le sprite horizontalement
             transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1f, 1f);
        }
    }
}