using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float dashSpeed; // Tốc độ dash
    [SerializeField] private float dashDuration; // Thời gian dash
    [SerializeField] private GameObject dashEffectPrefab; // Prefab cho hiệu ứng dash

    private Transform groundCheck;
    private float checkRadius = 0.6f;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;
    private float horizontalInput;
    private bool canMove = true;
    private bool isDashing = false;
    private bool isDizzy = false;

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
        // Nếu không đang Dash và có thể di chuyển, xử lý di chuyển và các hành động khác
        if (canMove && !isDashing)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            // Di chuyển nhân vật theo trục X
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

            if (!isGrounded() && body.velocity.y < 0)
            {
                body.gravityScale = 5; // Trọng lực nhẹ hơn khi rơi xuống
            }
            else
            {
                body.gravityScale = 7; // Trọng lực bình thường
            }

            if (horizontalInput > 0.01f)
            {
                transform.localScale = Vector3.one;
            }
            else if (horizontalInput < -0.01f)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }

            // Đặt hoạt ảnh chạy hoặc idle chỉ khi không dash
            if (!isDashing)
            {
                anim.SetBool("Run", horizontalInput != 0);
            }
            anim.SetBool("Grounded", isGrounded());

            // Dash khi nhấn phím Shift trái
            if (Input.GetKeyDown(KeyCode.LeftShift) && !isDizzy)
            {
                StartCoroutine(Dash());
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
        if (Input.GetButtonDown("Jump") && isGrounded())
        {
            Jump();
        }
    }

    private void Jump()
    {
        if (isGrounded())
        {
            body.velocity = new Vector2(body.velocity.x, jumpPower);
            anim.SetTrigger("Jump");
            body.gravityScale = 7;
        }
    }

    private void Crouch()
    {
        anim.SetBool("isCrouching", true);
    }

    private bool isGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    public bool canAttack()
    {
        return horizontalInput == 0 && isGrounded();
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
        isDashing = true;
        canMove = false; // Ngăn không cho di chuyển khi đang Dash

        // Đặt vận tốc dash ngay lập tức và kích hoạt hoạt ảnh dash
        float dashDirection = transform.localScale.x; // Hướng dash dựa vào hướng nhân vật đang đối mặt
        body.velocity = new Vector2(dashDirection * dashSpeed, body.velocity.y); // Set vận tốc dash (giữ lại tốc độ trục Y)
        anim.SetTrigger("Dash"); // Kích hoạt trigger cho hoạt ảnh dash
        anim.SetBool("isDashing", true); // Set trạng thái để chuyển sang hoạt ảnh dash

        // Tạo hiệu ứng dash ngay lập tức
        if (dashEffectPrefab != null)
        {
            GameObject effect = Instantiate(dashEffectPrefab, transform.position, Quaternion.identity);
            effect.transform.localScale = transform.localScale; // Flip hiệu ứng theo hướng nhân vật
            Vector3 effectPosition = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
            effect.transform.position = effectPosition;

            Animator effectAnimator = effect.GetComponent<Animator>();
            if (effectAnimator != null)
            {
                AnimatorClipInfo[] clipInfo = effectAnimator.GetCurrentAnimatorClipInfo(0);
                if (clipInfo.Length > 0)
                {
                    float animationLength = clipInfo[0].clip.length;
                    Destroy(effect, animationLength); // Xóa hiệu ứng sau khi hoạt ảnh kết thúc
                }
            }
            else
            {
                Destroy(effect, dashDuration); // Thời gian tồn tại của hiệu ứng bằng thời gian dash
            }
        }
        else
        {
            Debug.LogWarning("DashEffectPrefab is not assigned.");
        }

        yield return new WaitForSeconds(dashDuration); // Đợi thời gian dash

        // Sau khi dash, chuyển trạng thái nhân vật về lại bình thường
        isDashing = false;
        canMove = true; // Cho phép di chuyển trở lại
        anim.SetBool("isDashing", false); // Đặt biến IsDashing về false để chuyển về trạng thái idle

        // Đảm bảo nhân vật về trạng thái Idle hoặc Run tùy thuộc vào điều kiện
        anim.SetBool("Run", horizontalInput != 0);
        anim.ResetTrigger("Dash"); // Reset trigger để ngăn ngừa dash liên tục
    }
}
