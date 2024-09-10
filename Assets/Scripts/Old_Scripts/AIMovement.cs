using System.Collections;
using UnityEngine;

public class AIMovement : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float flykickSpeed;
    [SerializeField] private float castCooldown;
    [SerializeField] private float delayCasting;
    [SerializeField] private float strikeSpeed;
    [SerializeField] private float strikeDuration;
    [SerializeField] private float flykickDuration;
    [SerializeField] private GameObject EffectPrefab;
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
    private bool isFlyKicking = false;
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
        if (canMove && !isFlyKicking)
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
            StartCoroutine(FlyKick());
        if (Input.GetKeyDown(KeyCode.S) && isGrounded() && !isDizzy)
            StartCoroutine(CrouchRoutine());
        if (Input.GetKey(KeyCode.P) && !isDizzy)
            StartCoroutine(DizzyRoutine());
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded())
            Jump();
        if (isJumping && Input.GetMouseButtonDown(0))
            JumpATK();
        if (isGrounded() && Input.GetMouseButtonDown(0) && !isJumping)
            Attack();
        if (Input.GetMouseButton(1))
            StartCoroutine(Strike());
        if (Input.GetKeyDown(KeyCode.E))
            Win();
        if (Input.GetKeyDown(KeyCode.F))
            Strike();
        if (Input.GetKeyDown(KeyCode.R))
            Die();
        if (Input.GetKeyDown(KeyCode.Q))
            Hurt();
    }
    private void Jump()
    {
        if (isGrounded() && !isDizzy && !isFlyKicking && !isCrouching && !isStriking)
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
            StartCoroutine(DelayAttackTime());
        }
    }
    private void CrouchATK()
    {
        if (isCrouching)
        {
            anim.SetBool("isCrouching", true);
            anim.SetBool("isCrouchingATK", true);
            StartCoroutine(DelayAttackTime());
        }
    }
    private IEnumerator CrouchRoutine()
    {
        isCrouching = true;
        anim.SetBool("isCrouching", true);
        anim.SetBool("isCrouchingATK", false);
        canMove = false;

        while (Input.GetKey(KeyCode.S))
        {
            if (Input.GetMouseButtonDown(0))
            {
                CrouchATK();
                yield return new WaitForSeconds(0.5f);
                anim.SetBool("isCrouchingATK", false);
            }
            yield return null;
        }

        isCrouching = false;
        anim.SetBool("isCrouching", false);
        canMove = true;
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
    private IEnumerator FlyKick()
    {
        if (isGrounded() && !isDizzy && !isFlyKicking && canMove && !anim.GetCurrentAnimatorStateInfo(0).IsName("FlyKick"))
        {
            isFlyKicking = true;
            canMove = false;

            float flykickDirection = transform.localScale.x + 1f;
            float flykickAmount = flykickSpeed;
            body.velocity = new Vector2(flykickAmount * flykickDirection, body.velocity.y);

            anim.SetTrigger("FlyKick");
            anim.SetBool("isFlyKicking", true);

            if (EffectPrefab != null)
            {
                GameObject effect = Instantiate(EffectPrefab, transform.position, Quaternion.identity);
                effect.transform.localScale = transform.localScale;
                effect.transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);

                Animator effectAnimator = effect.GetComponent<Animator>();
                float animationLength = effectAnimator ? effectAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length : flykickDuration;
                Destroy(effect, animationLength);
            }

            yield return new WaitForSeconds(flykickDuration);

            isFlyKicking = false;
            canMove = true;
            anim.SetBool("isFlyKicking", false);
            anim.ResetTrigger("FlyKick");
        }
    }
    private void Attack()
    {
        if (cooldownTimer >= castCooldown && !isJumping && !isDizzy && !isFlyKicking && canMove && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            cooldownTimer = 0;
            anim.SetTrigger("Attack");
            anim.SetBool("isAttacking", true);
            canMove = false;
            StartCoroutine(DelayAttackTime());
            StartCoroutine(ResetAction("isAttacking"));
        }
    }
    private IEnumerator Strike()
    {
        if (isGrounded() && !isDizzy && canMove && !isStriking)
        {
            isStriking = true;
            canMove = false;

            float strikeDirection = transform.localScale.x + 1f;
            float strikeAmount = strikeSpeed;
            body.velocity = new Vector2(strikeDirection * strikeAmount, body.velocity.y);

            anim.SetTrigger("Strike");
            anim.SetBool("isStriking", true);
            if (EffectPrefab != null)
            {
                GameObject effect = Instantiate(EffectPrefab, transform.position, Quaternion.identity);
                effect.transform.localScale = transform.localScale;
                effect.transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);

                Animator effectAnimator = effect.GetComponent<Animator>();
                float animationLength = effectAnimator ? effectAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length : strikeDuration;
                Destroy(effect, animationLength);
            }

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
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.W))
            body.gravityScale = 10000;
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.F) || Input.GetMouseButton(1))
            body.gravityScale = 1;
        else
            body.gravityScale = 7;
    }
    public bool canAttack()
    {
        return horizontalInput == 0;
    }
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red; // Chọn màu cho Gizmo (ở đây là màu đỏ)
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius); // Vẽ một vòng tròn với bán kính `checkRadius` tại vị trí `groundCheck`
        }
    }

    private IEnumerator DelayAttackTime()
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
