using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truck : MonoBehaviour
{
     public float moveSpeed = 2f;
    private bool isMoving = true;

    void Update()
    {
        if (isMoving)
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
         if(collision.gameObject.CompareTag("Zombie"))
            isMoving = false;
    }
}
