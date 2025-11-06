using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public int lastDirection;
    public Vector2 direction;
    public float moveSpeed;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastDirection = 1;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x + direction.x * moveSpeed * Time.deltaTime, transform.position.y, transform.position.z);
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<Vector2>();

        if (direction.x > 0)
        {
            lastDirection = 1;
            spriteRenderer.flipX = true;
            animator.SetBool("Moving", true);
        }
        else if (direction.x < 0)
        {
            lastDirection = -1;
            spriteRenderer.flipX = false;
            animator.SetBool("Moving", true);
        }
        else
        {
            animator.SetBool("Moving", false);
        }
    }
}
