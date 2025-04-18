using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [Header("좀비 Prefabs")]
    public List<GameObject> zombiePrefabs;

    [Header("Pool 개수")]
    public int poolSizePerPrefab = 5;

    [Header("총알 Prefabs")]
    public GameObject bulletPrefab;

    [Header("총알 Pool 개수")]
    public int bulletPoolSize = 30;

    [Header("Pool 설정")]
    public Transform spawnPoint;
    public float spawnInterval = 3f;
    public int spawnCountPerInterval = 3;

    private Dictionary<GameObject, Queue<GameObject>> poolDict = new Dictionary<GameObject, Queue<GameObject>>();
    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        foreach (GameObject prefab in zombiePrefabs)
        {
            Queue<GameObject> queue = new Queue<GameObject>();

            for (int i = 0; i < poolSizePerPrefab; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }

            poolDict[prefab] = queue;
        }

        for (int i = 0; i < bulletPoolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }

        StartCoroutine(SpawnLoop());
    }

    /// <summary>
    /// spawnInterval 간격으로 좀비 소환
    /// </summary>
    /// <returns></returns>
    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            for (int i = 0; i < spawnCountPerInterval; i++)
            {
                SpawnRandomZombie();
            }
        }
    }

    /// <summary>
    /// Zombie 랜덤 생성
    /// </summary>
    void SpawnRandomZombie()
    {
        GameObject prefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Count)];

        GameObject zombie = GetFromPool(prefab);

        if (zombie != null)
        {
            zombie.transform.position = spawnPoint.position;
            zombie.transform.rotation = Quaternion.identity;
            zombie.SetActive(true);
        }
    }

    /// <summary>
    /// Pool에서 소환하기
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    GameObject GetFromPool(GameObject prefab)
    {
        if (poolDict.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            if (queue.Count > 0)
            {
                GameObject obj = queue.Dequeue();
                return obj;
            }
            else
            {
                GameObject obj = Instantiate(prefab, transform);
                return obj;
            }
        }

        return null;
    }

    /// <summary>
    /// Pool로 리턴
    /// </summary>
    /// <param name="obj"></param>
    public void ReturnToPool(GameObject obj)
    {
        foreach (var pair in poolDict)
        {
            if (obj.name.StartsWith(pair.Key.name))
            {
                obj.SetActive(false);
                poolDict[pair.Key].Enqueue(obj);
                return;
            }
        }

    }

    /// <summary>
    /// 총알 소환
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    public GameObject SpawnBullet(Vector3 position, Quaternion rotation, float speed)
    {
        GameObject bullet = (bulletPool.Count > 0) ? bulletPool.Dequeue() : Instantiate(bulletPrefab, transform);

        bullet.transform.position = position;
        bullet.transform.rotation = rotation;
        bullet.SetActive(true);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = bullet.transform.right * speed;

        return bullet;
    }

    /// <summary>
    /// 총알 리턴
    /// </summary>
    /// <param name="bullet"></param>
    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}
