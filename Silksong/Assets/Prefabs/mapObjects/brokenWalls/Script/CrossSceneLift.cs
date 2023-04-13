using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossSceneLift : SaveLift
{
    public bool UpLift;
    public string pairLiftGuid;
    public string Guid;
    int pairFloor;
    private void OnValidate()
    {
        Guid = GetComponent<GuidComponent>().GetGuid().ToString();
    }
    private void Awake()
    {
        IntGamingSave gamingSave;
        if (TryGetComponent(out gamingSave))
        {
            Debug.Log("try");
            bool error;
            int savedFloor = gamingSave.loadGamingData(out error);
            pairFloor = GetPairLiftFloor(out error);
            Debug.Log(error);
            if (error) return;
            currentFloor = savedFloor;
            Debug.Log("cf:" + currentFloor + " pf" + pairFloor);
            LoadFloor(currentFloor, pairFloor);
        }
    }
    public void SaveSelfFloor(int floor)
    {
        IntGamingSave gamingSave;
        if (TryGetComponent(out gamingSave))
        {
            gamingSave.saveGamingData(floor);
        }
    }  
    public void SavePairFloor(int floor)
    {
        GameManager.Instance.gamingSave.addIntGamingData(floor, pairLiftGuid);
    }
    public void SaveDownFloor()
    {
        if (pairFloor == 0)
        {
            return;
        }
        if(transform.position.y> floorList[1].transform.position.y)
        {
            SavePairFloor(2);
            if (currentFloor == 0) SaveSelfFloor(1);
        }
        else
        {
            SavePairFloor(1);
        }
    }
    public void MoveToFloor_Multy(int target)
    {
        int dir=target>currentFloor ? 1 : -1;
        StartCoroutine(Moving_Multy(currentFloor+dir,Mathf.Abs(target-currentFloor),dir));
    }
    IEnumerator Moving_Multy(int nextFloor, int time, int dir)
    {
        SaveSelfFloor(nextFloor);
        Debug.Log("save "+nextFloor + "  " + time + "  ");
        Vector2 target = floorList[nextFloor].transform.position;
        ifMoving = true;
        Vector2 moveTarget = new Vector2(0, target.y - transform.position.y).normalized;
        rb.velocity = moveTarget * moveSpeed;
        while (Mathf.Abs(transform.position.y - target.y) > 0.1)
        {
            //transform.position = Vector2.MoveTowards(transform.position,
            //    moveTarget, moveSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        rb.velocity = Vector2.zero;
        time--;
        if (time > 0)
        {
            StartCoroutine(Moving_Multy(nextFloor + dir, time, dir));
        }
    }


    public void LoadFloor(int currentFloor,int pairFloor)
    {
        if (UpLift)//
        {
            if (pairFloor == 0)//如果下层电梯在第0层 说明下层根本没动，那么这个电梯也就设置在第0层（隐藏层）
            {
                currentFloor = 0;
                SaveSelfFloor(0);
                transform.position = new Vector2(transform.position.x, floorList[currentFloor].transform.position.y);
            }
            else if (pairFloor == 2)//如果下层电梯在第2层，说明下层电梯已经上来了
            {
                //这个时候如果当前层在2，说明之前就在第二层了，正常设置在2就行
                if (currentFloor == 2)
                {
                    transform.position = new Vector2(transform.position.x, floorList[currentFloor].transform.position.y);
                }
                //如果不在2，说明电梯正在从下方往上走，这个时候电梯要从1上升到2
                else
                {
                    transform.position = new Vector2(transform.position.x, floorList[currentFloor].transform.position.y);
                    MoveToFloor(2);
                }
            }
            //如果下层是1，说明从半路往上爬了，这个时候就是从0层往2层走了
            else if (pairFloor == 1)
            {
                currentFloor = 0;
                transform.position = new Vector2(transform.position.x, floorList[currentFloor].transform.position.y);
                MoveToFloor(2);
            }
        }
        else//如果是下层电梯
        {
            //在第几层就先设置到第几层
            transform.position = new Vector2(transform.position.x, floorList[currentFloor].transform.position.y);
            if (pairFloor == 2)//如果上层电梯在第二层，那么下层电梯肯定在第二层（隐藏层）
            {
                currentFloor = 2;
                SaveSelfFloor(2);
                transform.position = new Vector2(transform.position.x, floorList[currentFloor].transform.position.y);
            }
            //如果是1的话，说明是正常下来的，这时候从1下降到0
            else if ( (pairFloor==0&&currentFloor==1))
            {
                MoveToFloor(0);
            }
            //如果上层是在1层的时候加载，说明半路跳车了，这个时候从2下降到0
            else if (pairFloor == 1)
            {
                currentFloor = 2;
                transform.position = new Vector2(transform.position.x, floorList[2].transform.position.y);
                MoveToFloor(0);
            }
        }
    }
    private int GetPairLiftFloor(out bool ifError)
    {
        return GameManager.Instance.gamingSave.getIntGamingData(pairLiftGuid, out ifError);
    }
}
