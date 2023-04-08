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
        GetComponent<Rigidbody2D>().velocity += new Vector2(0, UnityEngine.Random.Range(-4f, 4f) * Time.deltaTime);
        transform.Rotate(0, 0, UnityEngine.Random.Range(-50f, 50f) * Time.deltaTime);
    }
}
