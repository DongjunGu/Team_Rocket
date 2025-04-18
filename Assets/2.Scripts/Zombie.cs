using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Zombie : MonoBehaviour
{
    [Header("공격대상")]
    public GameObject target;

    [Header("움직임")]
    public float moveSpeed = 2f;
    public float jumpForce = 5f;
    public float jumpCooldown = 0.5f;
    private Rigidbody2D rb;
    private Animator anim;
    private float lastJumpTime = -10f;
    public bool shouldJump = false;
    public bool canPush = false;
    public bool canMove = true;

    [Header("공격과 밀기 조건")]
    public float damage = 0.007f;
    public float pushCooldownTime = 1.0f;
    private Dictionary<Transform, float> pushCooldowns = new Dictionary<Transform, float>();
    private bool isKnockback = false;
    private float knockbackDuration = 0.3f;
    private float knockbackTimer = 0f;

    [Header("물리 조건")]
    public float overlapThreshold = 0.3f;
    public float groundCheckThreshold = 0.1f;
    [Header("체력 바")]
    public Slider hpSlider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (hpSlider.value <= 0)
        {
            Dead();
        }
    }
    void FixedUpdate()
    {
        if (isKnockback)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockback = false;
            }
            return;
        }
        //이동
        if (!anim.GetBool("IsAttacking") && canMove)
        {
            rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
        }


        if (shouldJump && Time.time - lastJumpTime > jumpCooldown)
        {
            Jump();
            shouldJump = false;
        }

        TryPushBelow();

        TryJumpForward();

    }

    /// <summary>
    /// 위로 점프
    /// </summary>
    void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        lastJumpTime = Time.time;
    }

    /// <summary>
    /// 캐릭터 아래에 있는 다른 Zombie를 RayCast로 감지하고, 조건을 만족하면 밀기
    /// </summary>
    void TryPushBelow()
    {
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        Vector2 origin = transform.position;
        float rayLength = 0.2f;

        if (col != null)
        {
            float offsetX = col.size.x * 0.3f + 0.01f;
            origin = (Vector2)transform.position + Vector2.left * offsetX;
        }

        int layerMask = 1 << gameObject.layer;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, rayLength, layerMask);
        Debug.DrawRay(origin, Vector2.down * rayLength, Color.green);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.transform != transform)
            {
                Transform otherZombie = hit.collider.transform;

                Zombie otherMovement = otherZombie.GetComponent<Zombie>();
                if (otherMovement != null && otherMovement.target != null)
                {
                    bool canPush = !pushCooldowns.ContainsKey(otherZombie) ||
                                   Time.time - pushCooldowns[otherZombie] > pushCooldownTime;

                    if (canPush && transform.position.y > otherZombie.position.y)
                    {
                        CapsuleCollider2D otherCol = otherZombie.GetComponent<CapsuleCollider2D>();
                        if (otherCol != null)
                        {
                            float offsetX = otherCol.size.x + 0.5f;
                            StartCoroutine(SmoothPush(otherZombie, offsetX, 0.5f));
                            pushCooldowns[otherZombie] = Time.time;
                        }
                    }
                }
                break;
            }
        }
    }

    /// <summary>
    /// 캐릭터 앞에 다른 Zombie가 있는지 RayCast로 확인하고, 공격 중이거나 속도가 더 빠르면 점프
    /// </summary>
    void TryJumpForward()
    {
        if (Time.time - lastJumpTime < jumpCooldown)
            return;
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();

        Vector2 origin = transform.position;
        float rayLength = 0.1f;

        if (col != null)
        {
            float offsetX = col.size.x * 0.5f + 0.01f;
            origin = (Vector2)transform.position + Vector2.left * offsetX + Vector2.up * 0.5f;
            rayLength = Mathf.Abs(col.size.x * transform.localScale.x) / 2f;
        }

        Vector2 dir = Vector2.left;
        if (transform.localScale.x < 0) dir = Vector2.right;

        int layerMask = 1 << gameObject.layer;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, rayLength, layerMask);
        Debug.DrawRay(origin, dir * rayLength, Color.red);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.transform != transform)
            {
                Zombie otherZombie = hit.collider.GetComponent<Zombie>();

                if (otherZombie != null)
                {
                    if (anim.GetBool("IsAttacking"))
                    {
                        shouldJump = true;
                        break;
                    }

                    float mySpeed = Mathf.Abs(rb.velocity.x);
                    float otherSpeed = Mathf.Abs(otherZombie.GetComponent<Rigidbody2D>().velocity.x);

                    if (otherSpeed < mySpeed)
                    {
                        shouldJump = true;
                        break;
                    }
                }
            }
        }
    }


    /// <summary>
    /// 공격대상과 충돌 시 공격
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Hero"))
        {
            target = collision.gameObject;
            anim.SetBool("IsAttacking", true);
        }
    }

    /// <summary>
    /// 벗어나면 target초기화
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Hero"))
        {
            anim.SetBool("IsAttacking", false);
            target = null;
        }
    }

    /// <summary>
    /// Zombie를 부드럽게 밀어내고 이동시킴
    /// </summary>
    /// <param name="target"></param>
    /// <param name="offsetX"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator SmoothPush(Transform target, float offsetX, float duration)
    {
        Zombie zm = target.GetComponent<Zombie>();
        if (zm != null)
            zm.canMove = false;

        Vector3 startPos = target.position;
        Vector3 endPos = new Vector3(startPos.x + offsetX, startPos.y, startPos.z);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        target.position = endPos; // 보정
        if (zm != null)
            zm.canMove = true;
    }

    /// <summary>
    /// 상자 파괴 시 붙어있으면 약간 넉백
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="force"></param>
    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
            isKnockback = true;
            knockbackTimer = knockbackDuration;
        }
    }

    /// <summary>
    /// 대미지 입기
    /// </summary>
    /// <param name="amount"></param>
    public void TakeDamage(float amount)
    {
        if (!hpSlider.gameObject.activeSelf)
            hpSlider.gameObject.SetActive(true);

        hpSlider.value -= amount;
    }

    /// <summary>
    /// Attack Animation event로 공격시 발동
    /// </summary>
    public void OnAttack()
    {
        if (target != null)
        {
            target.GetComponent<Box>().TakeDamage(damage);
        }
    }

    /// <summary>
    /// 죽는 애니메이션 후 Pool로 리턴
    /// </summary>
    private void Dead()
    {
        anim.SetBool("IsDead", true);
        PoolManager.Instance.ReturnToPool(gameObject);
    }
}

