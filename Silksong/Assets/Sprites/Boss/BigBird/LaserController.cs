using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{

    public List<GameObject> Lasers;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void FireLaser()
    {
        foreach (var VARIABLE in Lasers)
        {
            VARIABLE.SetActive(true);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
