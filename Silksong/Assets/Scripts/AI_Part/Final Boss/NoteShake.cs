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
        GetComponent<Rigidbody2D>().velocity += new Vector2(0, UnityEngine.Random.Range(-2f, 2f) * Time.deltaTime);
        transform.Rotate(0, 0, UnityEngine.Random.Range(-30f, 30f) * Time.deltaTime);
    }
}
