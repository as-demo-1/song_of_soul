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

    IEnumerator ResetLasers()
    {
        yield return new WaitForSeconds(4);

        ResetLaser();
    }

    public void FireLaser()
    {
        foreach (var VARIABLE in Lasers)
        {
            VARIABLE.SetActive(true);
        }

        StartCoroutine(ResetLasers());
    }

    public void ResetLaser()
    {
        foreach (var VARIABLE in Lasers)
        {
            VARIABLE.SetActive(false);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
