using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Các biến SerializeField cho phép điều chỉnh các giá trị trong Unity Editor
    [SerializeField] private LayerMask groundLayer; // Lớp đất để kiểm tra xem nhân vật có chạm đất không
    [SerializeField] private float speed; // Tốc độ di chuyển của nhân vật
    [SerializeField] private float jumpPower; // Lực nhảy của nhân vật
    [SerializeField] private float dashSpeed; // Tốc độ khi Dash
    [SerializeField] private float dashDuration; // Thời gian kéo dài của Dash
    [SerializeField] private GameObject dashEffectPrefab; // Prefab cho hiệu ứng Dash
    [SerializeField] private float castCooldown;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject fireBallsPrefabs;
    [SerializeField] private float delayCasting;
    private float cooldownTimer;

    private Transform groundCheck; // Đối tượng kiểm tra xem nhân vật có chạm đất không
    private float checkRadius = 0.55f; // Bán kính kiểm tra chạm đất
    private Animator anim; // Animator để điều khiển các hoạt ảnh
    private BoxCollider2D boxCollider; // Collider để kiểm tra các va chạm
    private Rigidbody2D body; // Rigidbody2D để điều khiển vật lý của nhân vật
    private float horizontalInput; // Nhận input di chuyển ngang
    private bool canMove = true; // Cờ cho phép di chuyển
    private bool isDashing = false; // Cờ kiểm tra xem nhân vật đang Dash không
    private bool isDizzy = false; // Cờ kiểm tra xem nhân vật đang bị choáng không
    private bool isCrouching = false; // Cờ kiểm tra xem nhân vật đang Crouch không

    private void Awake()
    {
        // Khởi tạo các thành phần khi bắt đầu game
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Tạo đối tượng kiểm tra chạm đất và gán vị trí
        groundCheck = new GameObject("GroundCheck").transform;
        groundCheck.parent = transform;
        groundCheck.localPosition = new Vector3(0, -boxCollider.bounds.extents.y, 0);
    }

    private void Update()
    {
        // Nếu có thể di chuyển và không đang Dash, xử lý các hành động
        if (canMove && !isDashing)
        {
            HandleMovement(); // Xử lý di chuyển
            HandleAnimations(); // Xử lý hoạt ảnh
            HandleActions(); // Xử lý các hành động khác (nhảy, crouch, dizzy, dash)
        }

        AdjustGravityScale(); // Điều chỉnh trọng lực dựa vào trạng thái rơi
    }

    private void HandleMovement()
    {
        // Nhận input di chuyển ngang nếu không crouch
        if (!isCrouching)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

            // Thay đổi hướng nhân vật dựa vào hướng di chuyển
            if (horizontalInput > 0.01f)
                transform.localScale = Vector3.one; // Hướng về phía phải
            else if (horizontalInput < -0.01f)
                transform.localScale = new Vector3(-1, 1, 1); // Hướng về phía trái
        }
    }

    private void HandleAnimations()
    {
        // Cập nhật trạng thái hoạt ảnh dựa vào input và trạng thái chạm đất
        anim.SetBool("Run", horizontalInput != 0);
        anim.SetBool("Grounded", isGrounded());
        anim.SetBool("isCrouching", isCrouching);
    }

    private void HandleActions()
    {
        // Xử lý các hành động khi nhấn phím
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDizzy)
            StartCoroutine(Dash()); // Bắt đầu Coroutine Dash

        if (Input.GetKeyDown(KeyCode.S) && isGrounded() && !isDizzy)
            StartCoroutine(CrouchRoutine()); // Bắt đầu Coroutine Crouch

        if (Input.GetKey(KeyCode.P) && !isDizzy)
            StartCoroutine(DizzyRoutine()); // Bắt đầu Coroutine Dizzy

        if (Input.GetButtonDown("Jump") && isGrounded())
            Jump(); // Xử lý nhảy

        if (Input.GetMouseButtonDown(1))
        {
            Cast(); // Xử lý hành động Cast
        }
        cooldownTimer += Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
            Attack(); // Xử lý hành động Attack
    }

    private void Jump()
    {
        // Thực hiện nhảy nếu nhân vật đang chạm đất và không trong các trạng thái khác
        if (isGrounded() && !isDizzy && !isDashing && !isCrouching)
        {
            body.velocity = new Vector2(body.velocity.x, jumpPower); // Cập nhật vận tốc nhảy
            anim.SetTrigger("Jump"); // Kích hoạt trigger cho hoạt ảnh nhảy
        }
    }

    private IEnumerator CrouchRoutine()
    {
        // Xử lý hành động crouch
        isCrouching = true;
        anim.SetBool("isCrouching", true); // Bật hoạt ảnh crouch
        canMove = false; // Ngăn không cho di chuyển

        // Chờ cho đến khi phím S được thả ra
        yield return new WaitUntil(() => !Input.GetKey(KeyCode.S));

        isCrouching = false;
        anim.SetBool("isCrouching", false); // Tắt hoạt ảnh crouch
        canMove = true; // Cho phép di chuyển trở lại
    }

    private bool isGrounded()
    {
        // Kiểm tra xem nhân vật có đang chạm đất không
        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    private IEnumerator DizzyRoutine()
    {
        // Xử lý hành động bị choáng
        isDizzy = true;
        canMove = false; // Ngăn không cho di chuyển
        anim.SetBool("isDizzy", true); // Bật hoạt ảnh bị choáng
        yield return new WaitForSeconds(2); // Chờ 2 giây
        canMove = true; // Cho phép di chuyển trở lại
        anim.SetBool("isDizzy", false); // Tắt hoạt ảnh bị choáng
        isDizzy = false;
    }

    private IEnumerator Dash()
    {
        // Xử lý hành động Dash
        isDashing = true;
        canMove = false; // Ngăn không cho di chuyển

        // Cập nhật vận tốc Dash
        float dashDirection = transform.localScale.x; // Xác định hướng dash dựa vào hướng nhân vật
        body.velocity = new Vector2(dashDirection * dashSpeed, body.velocity.y); // Thiết lập vận tốc dash
        anim.SetTrigger("Dash"); // Kích hoạt trigger cho hoạt ảnh dash
        anim.SetBool("isDashing", true); // Đặt trạng thái Dash

        // Tạo hiệu ứng Dash nếu có
        if (dashEffectPrefab != null)
        {
            GameObject effect = Instantiate(dashEffectPrefab, transform.position, Quaternion.identity);
            effect.transform.localScale = transform.localScale; // Flip hiệu ứng theo hướng nhân vật
            effect.transform.position = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

            // Xóa hiệu ứng sau khi hoạt ảnh kết thúc
            Animator effectAnimator = effect.GetComponent<Animator>();
            float animationLength = effectAnimator ? effectAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length : dashDuration;
            Destroy(effect, animationLength);
        }

        yield return new WaitForSeconds(dashDuration); // Chờ thời gian Dash

        isDashing = false; // Đặt lại trạng thái Dash
        canMove = true; // Cho phép di chuyển trở lại
        anim.SetBool("isDashing", false); // Tắt trạng thái Dash
        anim.ResetTrigger("Dash"); // Reset trigger để ngăn ngừa Dash liên tục
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
        if (isGrounded() && !isDizzy && !isDashing && canMove && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            anim.SetTrigger("Attack");
            anim.SetBool("isAttacking", true);
            canMove = false;
            StartCoroutine(ResetAction("isAttacking"));
        }
    }

    private IEnumerator ResetAction(string actionBoolName)
    {
        // Xử lý reset trạng thái hành động (Casting, Attacking)
        if (actionBoolName == "isAttacking")
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
        // Điều chỉnh trọng lực khi nhân vật đang rơi để chuyển động mượt mà hơn
        if (!isGrounded() && body.velocity.y < 0)
            body.gravityScale = 5; // Giảm trọng lực khi rơi
        else if (Input.GetKey(KeyCode.S))
            body.gravityScale = 10000;
        else
            body.gravityScale = 7; // Khôi phục trọng lực bình thường
    }
    public bool canCast()
    {
        return horizontalInput == 0 && isGrounded();
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
