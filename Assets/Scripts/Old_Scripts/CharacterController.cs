using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float checkRadius = 0.55f;
    private Transform groundCheck;
    public float moveSpeed = 10f;
    public float jumpForce = 20f;
    private Rigidbody2D body;
    private Animator anim;
    private bool isJumping;
    private bool canMove = true;

    private bool isCrouching = false;
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Move();
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            Jump();
        }
    }

    private void Move()
    {
        if (!isCrouching)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

            if (horizontalInput > 0.01f)
                transform.localScale = Vector3.one;
            else if (horizontalInput < -0.01f)
                transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    private bool isGrounded()
    {
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (grounded)
        {
            isJumping = false;
            anim.SetBool("isJumping", false);
        }
        if (!grounded)
        {
            isJumping = true;
            anim.SetBool("isJumping", true);
        }
        return grounded;
    }


    void Jump()
    {
        if (isGrounded() && canMove)
        {
            isJumping = true;
            body.velocity = new Vector2(body.velocity.x, jumpPower);
            anim.SetTrigger("Jump");
            anim.SetBool("isJumping", true);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
        }
    }
}
