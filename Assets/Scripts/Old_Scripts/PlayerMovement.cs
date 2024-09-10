using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private float castCooldown;
    [SerializeField] private float delayCasting;
    [SerializeField] private float strikeSpeed;
    [SerializeField] private float strikeDuration;
    [SerializeField] private GameObject dashEffectPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject fireBallsPrefabs;



    private float cooldownTimer;

    private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.55f;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;
    private float horizontalInput;
    private bool canMove = true;
    private bool isDashing = false;
    private bool isDizzy = false;
    private bool isCrouching = false;
    private bool isStriking = false;
    private bool isJumping;


    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        groundCheck = new GameObject("GroundCheck").transform;
        groundCheck.parent = transform;
        groundCheck.localPosition = new Vector3(0, -boxCollider.bounds.extents.y, 0);
    }
    private void Update()
    {
        if (canMove && !isDashing)
        {
            HandleActions();
            HandleMovement();
            HandleAnimations();
        }
        cooldownTimer += Time.deltaTime;
        AdjustGravityScale();
    }
    private void HandleMovement()
    {
        if (!isCrouching)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

            if (horizontalInput > 0.01f)
                transform.localScale = Vector3.one;
            else if (horizontalInput < -0.01f)
                transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    private void HandleAnimations()
    {
        anim.SetBool("Run", horizontalInput != 0);
        anim.SetBool("Grounded", isGrounded());
        anim.SetBool("isCrouching", isCrouching);
    }
    private void HandleActions()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDizzy)
            StartCoroutine(Dash());
        if (Input.GetKeyDown(KeyCode.S) && isGrounded() && !isDizzy)
            StartCoroutine(CrouchRoutine());
        if (Input.GetKey(KeyCode.P) && !isDizzy)
            StartCoroutine(DizzyRoutine());
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded())
        {
            anim.SetBool("isJumping", true);
            Jump();
            Debug.Log("Jump");
        }
        if (isJumping && Input.GetMouseButtonDown(0))
        {
            JumpATK();
        }
        if (Input.GetMouseButtonDown(1))
            Cast();
        if (isGrounded() && Input.GetMouseButtonDown(0) && !isJumping)
            Attack();
        if (Input.GetKey(KeyCode.F))
            StartCoroutine(Strike());
        if (Input.GetKeyDown(KeyCode.W) && isGrounded() && !isDizzy)
            Block();
        if (Input.GetKeyDown(KeyCode.E))
            Win();
        if (Input.GetKeyDown(KeyCode.R))
            Die();
        if (Input.GetKeyDown(KeyCode.Q))
            Hurt();
    }
    private void Jump()
    {
        if (isGrounded() && !isDizzy && !isDashing && !isCrouching && !isStriking)
        {
            isJumping = true;
            body.velocity = new Vector2(body.velocity.x, jumpPower);
            anim.SetTrigger("Jump");
            anim.SetBool("isJumping", true);
        }
    }
    private void JumpATK()
    {
        if (isJumping && Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("JumpATK");
            anim.SetBool("isJumping", true);
        }
    }
    private IEnumerator CrouchRoutine()
    {
        isCrouching = true;
        anim.SetBool("isCrouching", true);
        canMove = false;

        yield return new WaitUntil(() => !Input.GetKey(KeyCode.S));

        isCrouching = false;
        anim.SetBool("isCrouching", false);
        canMove = true;
    }
    private void Block()
    {
        if (isGrounded() && !isDizzy && !isDashing && canMove)
        {
            canMove = false;
            anim.SetTrigger("Block");
            anim.SetBool("isBlocking", true);
            StartCoroutine(ResetAction("isBlocking"));
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
    private IEnumerator Dash()
    {
        if (isGrounded() && !isDizzy && !isDashing && canMove && !anim.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
        {
            isDashing = true;
            canMove = false;

            // Sử dụng giá trị dashSpeed làm tốc độ dash chính, và thêm một giá trị bổ sung để đảm bảo chuyển động
            float dashDirection = transform.localScale.x;
            float dashAmount = dashSpeed; // Đây là tốc độ dash chính
            body.velocity = new Vector2(dashDirection * dashAmount, body.velocity.y);

            anim.SetTrigger("Dash");
            anim.SetBool("isDashing", true);

            if (dashEffectPrefab != null)
            {
                GameObject effect = Instantiate(dashEffectPrefab, transform.position, Quaternion.identity);
                effect.transform.localScale = transform.localScale;
                effect.transform.position = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

                Animator effectAnimator = effect.GetComponent<Animator>();
                float animationLength = effectAnimator ? effectAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length : dashDuration;
                Destroy(effect, animationLength);
            }

            yield return new WaitForSeconds(dashDuration);

            isDashing = false;
            canMove = true;
            anim.SetBool("isDashing", false);
            anim.ResetTrigger("Dash");
        }
    }

    private void Cast()
    {
        if (cooldownTimer >= castCooldown && isGrounded() && !isDizzy && !isDashing && canMove && !anim.GetCurrentAnimatorStateInfo(0).IsName("Cast"))
        {
            cooldownTimer = 0;
            anim.SetTrigger("Cast");
            anim.SetBool("isCasting", true);
            canMove = false;
            StartCoroutine(DelayCastTime());
            StartCoroutine(ResetAction("isCasting"));
        }
    }
    private void Attack()
    {
        if (!isJumping && !isDizzy && !isDashing && canMove && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            anim.SetTrigger("Attack");
            anim.SetBool("isAttacking", true);
            canMove = false;
            StartCoroutine(ResetAction("isAttacking"));
        }
    }
    private IEnumerator Strike()
    {
        if (isGrounded() && !isDizzy && !isDashing && canMove && !isStriking)
        {
            isStriking = true;
            canMove = false;

            float strikeDirection = transform.localScale.x;
            body.velocity = new Vector2(strikeDirection * strikeSpeed, body.velocity.y);

            anim.SetTrigger("Strike");
            anim.SetBool("isStriking", true);

            yield return new WaitForSeconds(strikeDuration);

            isStriking = false;
            canMove = true;
            anim.SetBool("isStriking", false);
            anim.ResetTrigger("Strike");
        }
    }
    private void Hurt()
    {
        anim.SetTrigger("Hurt");
    }
    private void Win()
    {
        anim.SetTrigger("Win");
    }
    private void Die()
    {
        anim.SetTrigger("Die");
    }
    private IEnumerator ResetAction(string actionBoolName)
    {
        if (actionBoolName == "isAttacking")
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(0.3f);
            anim.SetBool(actionBoolName, false);
        }
        else if (actionBoolName == "isBlocking")
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(0.3f);
            anim.SetBool(actionBoolName, false);
        }
        else
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length);
            anim.SetBool(actionBoolName, false);
        }
        canMove = true;
    }
    private void AdjustGravityScale()
    {
        if (!isGrounded() && body.velocity.y < 0)
            body.gravityScale = 5;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.W) || Input.GetMouseButton(1))
            body.gravityScale = 10000;
        else
            body.gravityScale = 7;
    }
    public bool canCast()
    {
        return horizontalInput == 0 && isGrounded();
    }
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red; // Chọn màu cho Gizmo (ở đây là màu đỏ)
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius); // Vẽ một vòng tròn với bán kính `checkRadius` tại vị trí `groundCheck`
        }
    }

    private IEnumerator DelayCastTime()
    {
        yield return new WaitForSeconds(delayCasting);
        if (fireBallsPrefabs != null)
        {
            GameObject fireBalls = Instantiate(fireBallsPrefabs, firePoint.position, Quaternion.identity);
            Projecttile projecttileScript = fireBalls.GetComponent<Projecttile>();
            if (projecttileScript != null)
            {
                projecttileScript.SetDirection(transform.localScale.x);
            }
        }
    }
}
