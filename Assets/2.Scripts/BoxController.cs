using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    public float fallDuration = 0.2f;

    /// <summary>
    /// 파괴되었을때 fallDistance 만큼 떨어뜨림
    /// </summary>
    /// <param name="destroyed"></param>
    /// <param name="fallDistance"></param>
    public void MoveAboveObjectsDown(Transform destroyed, float fallDistance)
    {
        foreach (Transform child in transform)
        {
            if (child != destroyed && child.position.y > destroyed.position.y)
            {
                StartCoroutine(MoveDown(child, fallDistance, fallDuration));
            }
        }
    }

    /// <summary>
    /// Lerp사용해서 부드럽게 떨어뜨림
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="distance"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator MoveDown(Transform obj, float distance, float duration)
    {
        Vector3 start = obj.position;
        Vector3 end = start + Vector3.down * distance;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            obj.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        obj.position = end;
    }
}
