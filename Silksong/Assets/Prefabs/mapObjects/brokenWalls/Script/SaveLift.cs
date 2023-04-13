using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
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
    public int currentFloor;
    protected bool ifMoving;
    protected SaveSystem _saveSystem;
    public Rigidbody2D rb;
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
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb2= collision.transform.GetComponent<Rigidbody2D>();
        //Debug.Log(rb.velocity);
        if(rb2 != null)
            rb2.velocity =this.rb.velocity;
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
        Vector2 moveTarget= new Vector2(0, target.y-transform.position.y).normalized;
        rb.velocity = moveTarget * moveSpeed;
        while(Mathf.Abs(transform.position.y-target.y) > 0.1)
        {
            yield return new WaitForFixedUpdate();  
        }
        rb.velocity = Vector2.zero;
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
