using System;
using UnityEngine;


public class Move : MonoBehaviour
{
    private Vector3 pos;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pos = transform.position;
    }

    void FixedUpdate()
    {
        // Move in a Sin wave about the original pos
        transform.position += new Vector3((float)(Math.Sin(Time.time*2.0f)*0.05f), 0, (float)(Math.Sin(Time.time*2.0f)*0.05f) );

    }
}
