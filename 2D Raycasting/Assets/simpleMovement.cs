using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simpleMovement : MonoBehaviour
{
    public float moveSpeed = 1.0f;
    public float turnSpeed = 1.0f;
    private float delta = 0;
    private Vector3 Direction = new Vector3(1, 0, 0);
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        delta++;
        if(delta> turnSpeed * 100) 
        {
            Direction *= -1;
            delta = 0;
        }
        transform.position += Direction * Time.deltaTime * moveSpeed;
    }
}
