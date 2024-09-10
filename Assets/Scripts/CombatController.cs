using System.Collections;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    [Header("Fireball Settings")]
    public GameObject fireballPrefab;
    public Transform castPoint;
    public float castCooldown = 2.0f;

    [Header("Dash Effect")]
    public GameObject dashEffectPrefab;

    private bool canCast = true;
    private bool isStriking = false;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.F) && canCast)
        {
            StartCoroutine(CastFireball());
        }
    }

    private void Attack()
    {
        animator.SetTrigger("Attack");
    }

    private IEnumerator CastFireball()
    {
        canCast = false;
        animator.SetTrigger("Cast");
        yield return new WaitForSeconds(0.5f);
        Instantiate(fireballPrefab, castPoint.position, castPoint.rotation);
        yield return new WaitForSeconds(castCooldown);
        canCast = true;
    }

    public void PerformStrike()
    {
        if (isStriking) return;
        StartCoroutine(StrikeRoutine());
    }

    private IEnumerator StrikeRoutine()
    {
        isStriking = true;
        animator.SetTrigger("Strike");

        if (dashEffectPrefab != null)
        {
            Instantiate(dashEffectPrefab, transform.position, transform.rotation);
        }

        yield return new WaitForSeconds(1.0f);
        isStriking = false;
    }
}
