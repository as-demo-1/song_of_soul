using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThickLaserController : MonoBehaviour
{
    // Start is called before the first frame update
    private BoxCollider2D BoxCollider2D;
    void Start()
    {
        BoxCollider2D = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            Debug.Log("Damage");
        }
    }
}
