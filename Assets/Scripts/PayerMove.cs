using UnityEngine;
using UnityEngine.InputSystem;


public class PayerMove : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;
    private Vector2 move;

    [Header("Movement Settings")]
    public float spd = 1f;
    public float jumpForce = 5f;
    private bool facingRight = true;

    [Header("Collision Settings")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private bool isGrounded;
    [SerializeField] private LayerMask whatIsGround;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        HandleCollision();
        HandleFlip();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
                   move.x * spd,
                   rb.linearVelocity.y
        );
    }

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();

    }


    public void OnJump(InputValue value)
    {

        if (value.isPressed)
        {

            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    public void CharacterFlip()
    {
        if (move.x > 0 && !facingRight || move.x < 0 && facingRight)
        {
            facingRight = !facingRight;
            GetComponentInChildren<SpriteRenderer>().flipX = !GetComponentInChildren<SpriteRenderer>().flipX;
        }
    }
    private void HandleFlip()
    {
        if (rb.linearVelocity.x < 0 && facingRight == true) 
            FlipCharacter();
        else if (rb.linearVelocity.x > 0 && facingRight == false) 
            FlipCharacter();
    }

    private void FlipCharacter()
    {
        transform.Rotate(0,180, 0);
        facingRight = !facingRight;
    }

    private void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, - groundCheckDistance));
    }
}
