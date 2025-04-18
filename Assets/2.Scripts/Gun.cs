using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("총 발사 관련")]
    public Transform firePoint;
    public int pelletCount = 5;
    public float spreadAngle = 15f;
    public float bulletSpeed = 30f;
    public float fireInterval = 1f;

    [Header("총알 Prefab")]
    public GameObject bulletPrefab;
    private float fireTimer;
    private LineRenderer lr;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 3;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.white;
        lr.endColor = Color.white;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }
    void Update()
    {
        RotateTowardsMouse();

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            FireShotgun();
            fireTimer = 0f;
        }

        DrawSpreadArc();
    }

    /// <summary>
    /// firePoint와 Gun이 마우스를 향하도록 회전
    /// </summary>
    void RotateTowardsMouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector2 dir = (mouseWorldPos - firePoint.position).normalized;

        firePoint.right = dir;
        transform.right = dir;
    }

    /// <summary>
    /// 총알 발사
    /// </summary>
    void FireShotgun()
    {
        float halfSpread = spreadAngle / 2f;

        for (int i = 0; i < pelletCount; i++)
        {
            float angleOffset = Random.Range(-halfSpread, halfSpread);
            Quaternion bulletRotation = firePoint.rotation * Quaternion.Euler(0, 0, angleOffset);

            GameObject bullet = PoolManager.Instance.SpawnBullet(firePoint.position, bulletRotation, bulletSpeed);

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.velocity = bullet.transform.right * bulletSpeed;
        }
    }

    /// <summary>
    /// 부채꼴 그리기
    /// </summary>
    void DrawSpreadArc()
    {
        float halfSpread = spreadAngle / 2f;
        Vector3 center = firePoint.position;
        Vector3 leftDir = Quaternion.Euler(0, 0, -halfSpread) * firePoint.right;
        Vector3 rightDir = Quaternion.Euler(0, 0, halfSpread) * firePoint.right;

        float length = 3f;

        lr.SetPosition(0, center + leftDir * length);
        lr.SetPosition(1, center);
        lr.SetPosition(2, center + rightDir * length);
    }
}
