using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    public float fallDuration = 0.2f;

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
