using System.Collections;
using UnityEngine;

public class Projecttile : MonoBehaviour
{
    [SerializeField] private float speed;
    private float direction;
    private bool hit;
    private BoxCollider2D boxCollider;
    private Animator anim;
    private float lifetime = 5f;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        if (hit) return;
        float movementspeed = Time.deltaTime * speed * direction;
        transform.Translate(movementspeed, 0, 0);
        lifetime += Time.deltaTime;
        if (lifetime > 5) gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hit) return;
        hit = true;
        boxCollider.enabled = false;
        anim.SetTrigger("Explosion");
        StopCoroutine(LifetimeCroutine());
        StopCoroutine(WaitForExplosion());
    }

    private IEnumerator LifetimeCroutine()
    {
        yield return new WaitForSeconds(lifetime);
        Deactivate();
    }

    private IEnumerator WaitForExplosion()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);
        Deactivate();
    }
    public void SetDirection(float _direction) 
    {
        lifetime = 0;
        direction = _direction;
        gameObject.SetActive(true);
        hit = false;
        boxCollider.enabled = true;
        float localScaleX = transform.localScale.x;
        if (Mathf.Sign(localScaleX) != _direction)
            localScaleX = -localScaleX;
        transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);
    }
    private void Deactivate()
    {
        Destroy(gameObject);
    }
}
