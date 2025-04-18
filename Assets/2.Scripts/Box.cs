using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Box : MonoBehaviour
{
    public Slider hpSlider;
    public float knockbackForce = 3f;
    
    /// <summary>
    /// 공격받는 함수
    /// </summary>
    /// <param name="amount"></param>
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

    /// <summary>
    /// Box가 파괴되면서 크기만큼 아래로 하강 + 약간의 넉백 + 파괴
    /// </summary>
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
                Zombie zombie = hit.GetComponent<Zombie>();
                if (zombie != null)
                {
                    zombie.ApplyKnockback(Vector2.right, knockbackForce);
                }
            }
        }

        transform.parent.GetComponent<BoxController>().MoveAboveObjectsDown(this.transform, height);

        Destroy(gameObject);
    }
}

