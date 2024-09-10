using System.Collections;
using UnityEngine;

public class AbilityController : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private GameObject dashEffectPrefab;

    [Header("Jump Settings")]
    [SerializeField] private float jumpPower;

    private Rigidbody2D body;
    private Animator anim;
    private bool isDashing = false;
    private bool isJumping = false;
    private bool isDizzy = false;
    private bool isCrouching = false;
    private bool isStriking = false;
    private string selectedCharacter;
    private void Awake()
    {
        selectedCharacter = PlayerPrefs.GetString("SelectedCharacter");
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (selectedCharacter == "Knight")
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
            {
                StartCoroutine(Dash());
            }

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded())
            {
                Jump();
            }

        }
        else if (selectedCharacter == "Dragon")
        {

        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        float dashDirection = transform.localScale.x;
        body.velocity = new Vector2(dashDirection * dashSpeed, body.velocity.y);
        anim.SetTrigger("Dash");

        if (dashEffectPrefab != null)
        {
            GameObject effect = Instantiate(dashEffectPrefab, transform.position, Quaternion.identity);
            effect.transform.localScale = transform.localScale;
            Destroy(effect, dashDuration);
        }

        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
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

    private bool isGrounded()
    {
        bool grounded = Physics2D.OverlapCircle(transform.position, 0.55f, LayerMask.GetMask("Ground"));
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
}
