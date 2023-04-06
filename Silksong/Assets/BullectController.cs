using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class BullectController : MonoBehaviour
{
    private LaserBullect _bullect;
    public GameObject BullectPrefab;
    public GameObject BullectPrefabForSecFinal;
    public float timer = 0f;
    private float timecount = 0f;

    private bool shouldend = false;

    private bool startf = false;

    private bool rotate;
    // Start is called before the first frame update
    void Start()
    {
        _bullect = BullectPrefab.GetComponent<LaserBullect>();
    }

    public void start(bool shouldrotate)
    {
        startf = true;
        shouldend = false;
        timecount = 0f;
        rotate = shouldrotate;
    }
    void Func()
    {
        int x = Random.Range(-10, 10);
        int y = Random.Range(-10, 10);
        for (int i = 0; i < Random.Range(8, 11); i++)
        {
            GameObject.Instantiate(BullectPrefab,transform.position,quaternion.identity); 
        }
 
    }
    
    void Func2()
    {
        int x = Random.Range(-10, 10);
        int y = Random.Range(-10, 10);
        for (int i = 0; i < Random.Range(8, 11); i++)
        {
            GameObject.Instantiate(BullectPrefab,transform.position,quaternion.identity); 
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (startf)
        {
            timer += Time.deltaTime;

            timecount += Time.deltaTime;
            if (rotate)
            {
                if (timer >= 1 && !shouldend)
                {
                    Func();
                    timer = 0f; // 定时2秒
                }
                else if (timecount >= 15)
                {
                    shouldend = true;
                }
            }
            else
            {
                if (timer >= 1 && !shouldend)
                {
                    Func2();
                    timer = 0f; // 定时2秒
                }
                else if (timecount >= 15)
                {
                    shouldend = true;
                }
            }
        }
    }
    


}

