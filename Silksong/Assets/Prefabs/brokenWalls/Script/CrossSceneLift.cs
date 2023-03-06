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
        //_saveSystem = GameManager.Instance.saveSystem;
        IntGamingSave gamingSave;
        if (TryGetComponent(out gamingSave))
        {
            bool error;
            int savedFloor = gamingSave.loadGamingData(out error);
            if (error) return;
            pairFloor = GetPairLiftFloor(out error);
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
    public void LoadFloor(int currentFloor,int pairFloor)
    {
        if (UpLift)
        {
            if (pairFloor == 0)
            {
                currentFloor = 0;
                SaveSelfFloor(0);
                transform.position = new Vector2(transform.position.x, floorList[currentFloor].transform.position.y);
            }
            else if (pairFloor == 2)
            {
                if (currentFloor == 2)
                {
                    transform.position = new Vector2(transform.position.x, floorList[currentFloor].transform.position.y);
                }
                else
                {
                    transform.position = new Vector2(transform.position.x, floorList[currentFloor].transform.position.y);
                    MoveToFloor(2);
                }
            }
        }
        else
        {
            transform.position = new Vector2(transform.position.x, floorList[currentFloor].transform.position.y);
            if (pairFloor == 2)
            {
                currentFloor = 2;
                SaveSelfFloor(2);
                transform.position = new Vector2(transform.position.x, floorList[currentFloor].transform.position.y);
            }
            else if (pairFloor==1||(pairFloor==0&&currentFloor==1))
            {
                MoveToFloor(0);
            }
        }
    }
    private int GetPairLiftFloor(out bool ifError)
    {
        return GameManager.Instance.gamingSave.getIntGamingData(pairLiftGuid, out ifError);
    }
}
