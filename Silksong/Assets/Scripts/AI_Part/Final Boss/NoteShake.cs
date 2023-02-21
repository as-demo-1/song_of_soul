using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteShake : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 10 ==0)
            GetComponent<Rigidbody2D>().velocity += new Vector2(0, UnityEngine.Random.Range(-2f, 2f));
    }
}
