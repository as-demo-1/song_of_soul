using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeTime : MonoBehaviour
{
    // Start is called before the first frame update
    public float lifeCycle = 3f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLifeCycle(float time)
    {
        lifeCycle = time;
    }

    private void FixedUpdate()
    {
        lifeCycle -= Time.fixedDeltaTime;
        if (lifeCycle <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
