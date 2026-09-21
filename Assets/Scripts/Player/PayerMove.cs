using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField, Range(0f, 1f)] private float jumpSoundVolume = 0.2f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField, Range(0.1f, 1f)] private float doubleJumpForceMultiplier = 0.7f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField, Min(1)] private int maxJumps = 2; 

    private int jumpsUsed;
    private bool isJumping;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isFacingRight = true;
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private float moveInput;
    private float movementLockedUntil;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (IsGrounded() && rb.linearVelocity.y <= 0f)
            jumpsUsed = 0;
        
        Move();
        HandleFlip();
        AnimTransition();
    }

    private void AnimTransition()
    {
        animator.SetFloat("ValueX", Mathf.Abs(moveInput));
    }


    private void Move()
    {
        if (Time.time < movementLockedUntil)
            return;

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    public void ApplyKnockback(Vector2 velocity, float movementLockDuration)
    {
        movementLockedUntil = Mathf.Max(
            movementLockedUntil,
            Time.time + movementLockDuration);
        rb.linearVelocity = velocity;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>().x;
    }


    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            if (IsGrounded() && rb.linearVelocity.y <= 0f)
                jumpsUsed = 0;
        
            if (jumpsUsed < maxJumps)
            {
                float currentJumpForce = jumpsUsed == 0
                    ? jumpForce
                    : jumpForce * doubleJumpForceMultiplier;

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, currentJumpForce);
                jumpsUsed++;
                isJumping = true;

                if (audioSource != null && jumpSound != null)
                    audioSource.PlayOneShot(jumpSound, jumpSoundVolume);
                
            }
            return;
        }

        if (isJumping)
        {
            isJumping = false;

            if (rb.linearVelocity.y > 0f)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position,groundCheckRadius,groundLayer);
    }

    private void OnDrawGizmos()
    { 
        if (groundCheck == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position,groundCheckRadius);
    }
    private void HandleFlip()
    {

        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }
}
