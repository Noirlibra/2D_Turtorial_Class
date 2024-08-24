using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;
    private float wallJumpCooldown;
    private float horizontalInput;
    private bool canMove = true;
    private bool isDizzy = false;
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    private void Update()
    {
        if (canMove)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            if (horizontalInput > 0.01f)
            {
                transform.localScale = Vector3.one;
            }
            else if (horizontalInput < -0.01f)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }

            //Set doi tuong voi tham so
            anim.SetBool("Run", horizontalInput != 0);
            anim.SetBool("Grounded", isGrounded());
            //Tao do tre 2 lan nhay
            if (wallJumpCooldown < 0.2f)
            {
                body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
                if (onWall() && !isGrounded())
                {
                    body.gravityScale = 0;
                    body.velocity = Vector2.zero;
                }
                else body.gravityScale = 7;
                if (Input.GetKey(KeyCode.Space))
                    Jump();

            }
            else
            {
                wallJumpCooldown += Time.deltaTime;
            }
        }
        if (Input.GetKeyDown(KeyCode.S) && isGrounded() && !isDizzy)
        {
            Crouch();
            canMove = false;
            body.gravityScale = 10000;
        }
        if (Input.GetKeyUp(KeyCode.S) && isGrounded())
        {
            anim.SetBool("isCrouching", false);
            canMove = true;
            body.gravityScale = 7;
        }
        if (Input.GetKey(KeyCode.P) && !isDizzy)
        {
            StartCoroutine(DizzyRoutine());
        }
    }
    private void Jump()
    {
        if (isGrounded())
        {
            body.velocity = new Vector2(body.velocity.x, jumpPower);
            anim.SetTrigger("Jump");

        }
        else if (onWall() && !isGrounded())
        {
            if (horizontalInput == 0)
            {
                body.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * 10, 0);
                transform.localScale = new Vector3(-Mathf.Sign(transform.localScale.x), transform.localScale.y, transform.localScale.z);

            }
            else
                body.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * 3, 6);
            wallJumpCooldown = 0;
        }
    }
    private void Crouch()
    {
        anim.SetBool("isCrouching", true);
    }
    private bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }
    private bool onWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }
    public bool canAttack()
    {
        return horizontalInput == 0 && isGrounded() && !onWall();
    }
    private IEnumerator DizzyRoutine()
    {
        isDizzy = true;
        canMove = false;
        anim.SetBool("isDizzy", true);
        yield return new WaitForSeconds(2);
        canMove = true;
        anim.SetBool("isDizzy", false);
        isDizzy = false;
    }
}
