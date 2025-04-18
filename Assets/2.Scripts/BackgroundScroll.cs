using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Transform[] parts;
    private float spriteWidth;

    void Start()
    {
        parts = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            parts[i] = transform.GetChild(i);
        }
        SpriteRenderer sr = parts[0].GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            spriteWidth = sr.bounds.size.x;
        }
    }

    void Update()
    {
        foreach (Transform part in parts)
        {
            part.Translate(Vector2.left * moveSpeed * Time.deltaTime);

            if (part.position.x <= -spriteWidth)
            {
                float rightMostX = GetRightMostX();
                float smallOffset = 0.2f;
                part.position = new Vector3(rightMostX + spriteWidth - smallOffset, part.position.y, part.position.z);

            }
        }
    }

    /// <summary>
    /// Transform x 좌표중 오른쪽 위치값 반환
    /// </summary>
    /// <returns></returns>
    float GetRightMostX()
    {
        float maxX = float.MinValue;
        foreach (Transform t in parts)
        {
            if (t.position.x > maxX)
                maxX = t.position.x;
        }
        return maxX;
    }
}
