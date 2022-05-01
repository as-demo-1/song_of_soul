using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crack : Damager
{
    public float disappearTime;
    private void Update()
    {
        disappearTime-=Time.deltaTime;
        if(disappearTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
