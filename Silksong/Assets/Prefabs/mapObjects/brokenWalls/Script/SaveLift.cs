using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 可在场景切换后保存电梯状态的电梯
/// 保存还是有问题的
/// 作者：小明
/// </summary>
public class SaveLift : MonoBehaviour
{
    public List<SaveLiftFloor> floorList;
    public float moveSpeed;
    [HideInInspector]public int currentFloor;
    bool ifMoving;
    protected SaveSystem _saveSystem;
    private void Awake()
    {
        _saveSystem = GameManager.Instance.saveSystem;
        IntGamingSave gamingSave;
        if (TryGetComponent(out gamingSave))
        {
            bool error;
            int savedFloor = gamingSave.loadGamingData(out error);
            if (error) return;
            currentFloor = savedFloor;
            transform.position =new Vector2(transform.position.x, floorList[currentFloor].transform.position.y);
        }
    }
    public void MoveToFloor(int n)
    {
        if (!ifMoving)
        {
            currentFloor = n;
            StartCoroutine(Moving(floorList[n].transform.position));
            IntGamingSave gamingSave;
            if (TryGetComponent(out gamingSave))
            {
                gamingSave.saveGamingData(currentFloor);
            }
        }
    }
    public void MoveToFloor(SaveLiftFloor floor)
    {
        int n= floorList.IndexOf(floor);    
        MoveToFloor(n);
    }
    IEnumerator Moving(Vector2 target)
    {
        ifMoving = true;
        Vector2 moveTarget= new Vector2(transform.position.x, target.y);
        while(Mathf.Abs(transform.position.y-target.y) > 0.05)
        {
            transform.position = Vector2.MoveTowards(transform.position,
                moveTarget, moveSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();  
        }
        ifMoving=false;
    }
    public virtual void MovingUp()
    {
        if (currentFloor + 1 < floorList.Count)
        {
            MoveToFloor(currentFloor + 1);
        }
    }
    public virtual void MovingDown()
    {
        if(currentFloor -1 >= 0)
        {
            MoveToFloor(currentFloor - 1);
        }
    }
    
}
