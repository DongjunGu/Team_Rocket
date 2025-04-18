using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 2f;
    public float damage = 0.5f;

    void OnEnable()
    {
        CancelInvoke();
        Invoke(nameof(Deactivate), lifeTime);
    }

    /// <summary>
    /// 좀비가 맞을경우 대미지주기, 땅에 맞을 시 반환
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Zombie"))
        {
            other.GetComponent<Zombie>().TakeDamage(damage);
            Deactivate();
        }
        if (other.CompareTag("Ground"))
        {
            Deactivate();
        }
    }

    /// <summary>
    /// 총알 반환
    /// </summary>
    void Deactivate()
    {
        PoolManager.Instance.ReturnBullet(gameObject);
    }
}
