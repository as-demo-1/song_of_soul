using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class FireBullect : Action
{
    public GameObject BullectPrefab;
    private bool fire = false;
    private LaserBullect _bullect;
    // Start is called before the first frame update

    public int count = 6;

    public override void OnStart()
    {
        _bullect = BullectPrefab.GetComponent<LaserBullect>();

    }

    IEnumerator Fire()
    {
        int x = Random.Range(-10, 10);
        int y = Random.Range(-10, 10);
        Debug.Log(Owner.transform.position);
        GameObject.Instantiate(BullectPrefab,Owner.transform.position,Quaternion.identity);
       // _bullect.Setup(new Vector3(x,y,0));
        yield return new WaitForSeconds(5f);
        
    }
    // Update is called once per frame
    public float timer = 0f;
    private float timecount = 0f;
    void Update()
    {
      
    }
    void Func()
    {
        int x = Random.Range(-10, 10);
        int y = Random.Range(-10, 10);
        GameObject.Instantiate(BullectPrefab);
            //_bullect.Setup(new Vector3(x,y,0));
      //  Debug.Log("每2秒执行一次");
    }

    public override TaskStatus OnUpdate()
    {

        timer += Time.deltaTime;
        timecount += Time.deltaTime;
        if (timer >= 1)
        {
            Func();
            timer = 0f; // 定时2秒
        }
        else if (timecount >= 5)
        {
            fire = true;
            Debug.Log("end");
        }
        if (fire)
        {
            return TaskStatus.Inactive;
        }

        return TaskStatus.Running;
    }
}
