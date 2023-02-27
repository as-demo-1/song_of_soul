using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drum : MonoBehaviour
{
    public GameObject circle;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Generate()
    {
        for (int i = 0;i < transform.childCount; ++i)
        {
            Generate(i);
        }
    }

    void Generate(int idx)
    {
        GameObject pos = transform.GetChild(idx).gameObject;
        GameObject temp = Instantiate(circle, pos.transform);
        temp.transform.position = pos.transform.position;
        temp.transform.parent = pos.transform;
    }
}
