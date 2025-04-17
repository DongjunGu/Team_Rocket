using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Box : MonoBehaviour
{
    public Slider hpSlider;
    public float knockbackForce = 2f;
    public void TakeDamage(float amount)
    {
        if (hpSlider == null)
        {
            return;
        }

        hpSlider.value -= amount;
    }

    private void Update()
    {
        if (hpSlider.value <= 0)
        {
            DestroyBox();
        }
    }

    public void DestroyBox()
    {
        float height = GetComponent<BoxCollider2D>().size.y * transform.localScale.y;
        Vector2 boxSize = GetComponent<BoxCollider2D>().size;
        boxSize *= 1.1f;
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, boxSize, 0f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Zombie"))
            {
                ZombieMovement zombie = hit.GetComponent<ZombieMovement>();
                if (zombie != null)
                {
                    zombie.ApplyKnockback(Vector2.right, knockbackForce);
                }
            }
        }

        // 부모인 BoxTrans에게 알림
        transform.parent.GetComponent<BoxController>().MoveAboveObjectsDown(this.transform, height);

        Destroy(gameObject);
    }
}

