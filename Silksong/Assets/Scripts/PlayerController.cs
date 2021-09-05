using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float speed;
    //冲刺
    private bool isRun;
    public float runRate;
    float currSpeed;



    // Start is called before the first frame update
    void Start()
    {
        currSpeed = speed;

    }


    // Update is called once per frame
    void Update()
    {
        CheckIsRun();
    }

    private void FixedUpdate()
    {

    }
    /// <summary>
    /// //冲刺判断
    /// </summary>
    void CheckIsRun()
    {
        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRun = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isRun = false;
            speed = currSpeed;
        }
    }


}
