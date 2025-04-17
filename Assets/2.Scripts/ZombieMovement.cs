using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieMovement : MonoBehaviour
{
    public GameObject target;
    public float moveSpeed = 2f;
    public float jumpForce = 5f;
    public float jumpCooldown = 0.5f;
    private Rigidbody2D rb;
    private Animator anim;
    private float lastJumpTime = -10f;
    public bool shouldJump = false;
    public float damage = 0.1f;
    public float groundCheckThreshold = 0.1f;
    public float pushCooldownTime = 1.0f;  // 밀기 쿨타임
    private Dictionary<Transform, float> pushCooldowns = new Dictionary<Transform, float>();
    private bool isKnockback = false;
    private float knockbackDuration = 0.3f;
    private float knockbackTimer = 0f;
    public float overlapThreshold = 0.3f;
    public bool canPush = false;
    public bool canMove = true;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
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
            return; // 넉백 중이므로 이동 안 함
        }
        //왼쪽 이동
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

    void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        lastJumpTime = Time.time;
    }

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

                ZombieMovement otherMovement = otherZombie.GetComponent<ZombieMovement>();
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
                            StartCoroutine(SmoothPush(otherZombie, offsetX, 0.2f));
                            pushCooldowns[otherZombie] = Time.time;
                        }
                    }
                }
                break;
            }
        }
    }



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

        // ✅ 모든 히트 감지
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, rayLength, layerMask);
        Debug.DrawRay(origin, dir * rayLength, Color.red);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.transform != transform)
            {
                // ✅ 자신 제외한 첫 번째 만난 오브젝트 기준 점프
                if (transform.position.y <= hit.collider.transform.position.y)
                {
                    shouldJump = true;
                    break;
                }
            }
        }
    }



    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Hero"))
        {
            target = collision.gameObject;
            anim.SetBool("IsAttacking", true);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Hero"))
        {
            anim.SetBool("IsAttacking", false);
        }
    }
    IEnumerator SmoothPush(Transform target, float offsetX, float duration)
    {
        ZombieMovement zm = target.GetComponent<ZombieMovement>();
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

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // 기존 속도 제거
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
            isKnockback = true;
            knockbackTimer = knockbackDuration;
        }
    }
    public void OnAttack()
    {
        if (target != null)
        {
            target.GetComponent<Box>().TakeDamage(damage);
        }
    }
}

