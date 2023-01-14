using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class LaserFire : MonoBehaviour
{

    Vector3 Up = Vector3.up;
    Vector3 Down = Vector3.down;
    public Transform targetTF;
    // Start is called before the first frame update
    void Start()
    {
        targetTF = GameObject.FindGameObjectWithTag("Player").transform;
    }
    
    public void Rotate()
    {
        Quaternion dir = Quaternion.LookRotation(targetTF.position - this.transform.position);
        this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation,dir,1f );
      
    }

    // Update is called once per frame
    void Update()
    {
        Rotate();
    }
}
