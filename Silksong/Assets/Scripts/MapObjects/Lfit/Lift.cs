using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 电梯1.0
/// </summary>作者：青瓜
public class Lift : MonoBehaviour
{
    public int maxFloor;//从1开始计算

    private LiftFloorGear[] gears;//lift为该电梯的liftFloorGear在start时绑定到该数组  按从低层到高层顺序

#if UNITY_EDITOR 
    [DisplayOnly]
#endif
    public float currentFloor;//当前层 若为x.5层则表示正在x与x+1层之间移动

#if UNITY_EDITOR 
    [DisplayOnly]
#endif
    public int targetFloor;//最终要到的层

#if UNITY_EDITOR 
    [DisplayOnly]
#endif
    public int midTargetFloor;//现在层和目标层之间的中间层 用于在跨多层移动中记录电梯当前位置


    private float midFloorHeight;//中间层的地面y轴高度
    private float liftFloorDistance;

    public float speed;
    private float arriveDistance;//当电梯与目的地距离小于此值时 判定到达

    // private PlayerController player;
    private Rigidbody2D playerRigid;

    private Rigidbody2D rigid;
    private BoxCollider2D floorCollider;//电梯地面的碰撞体

#if UNITY_EDITOR 
    [DisplayOnly]
#endif
    public bool playerIsOnLift = false;//玩家是否在电梯上 用于同步速度

    void Awake()
    {
        gears = new LiftFloorGear[maxFloor];
        rigid = GetComponent<Rigidbody2D>();
        floorCollider = GetComponent<BoxCollider2D>();

        foreach (var gear in GetComponentsInChildren<LiftFloorGear>())
        {
            gear.lift = this;
            gears[gear.floor - 1] = gear;
        }

    }
    void Start()
    {
        GameObject playerobj = GameObject.FindGameObjectWithTag("Player");
        if (playerobj)
        {
            //player = playerobj.GetComponent<PlayerController>();
            playerRigid = playerobj.GetComponent<Rigidbody2D>();
        }
        arriveDistance = speed * Time.fixedDeltaTime;
        liftFloorDistance = floorCollider.offset.y;
        liftFloorDistance += floorCollider.bounds.extents.y;

        IntGamingSave gamingSave;
        if (TryGetComponent(out gamingSave) &&!gamingSave.ban)
        {
            bool error;
            int savedFloor = gamingSave.loadGamingData(out error);
            if (error) return;
            //Debug.Log(savedFloor);
            float targetFloorHeight = gears[savedFloor - 1].floorHeight;
            float distance = getFloorPosition() - targetFloorHeight;
            rigid.MovePosition(new Vector3(transform.position.x, transform.position.y - distance, transform.position.z));
            currentFloor = savedFloor;
        }
    }


    private float getFloorPosition()
    {
        return transform.position.y + liftFloorDistance;
    }

    private void Update()
    {
        playerIsOnLift = (floorCollider.IsTouchingLayers(1 << LayerMask.NameToLayer("Player")) && playerRigid.transform.position.y > getFloorPosition());

        if (rigid.velocity.y != 0)//电梯在移动
        {
            float distance = getFloorPosition() - midFloorHeight;
            //Debug.Log(distance);
            if (Mathf.Abs(distance) < arriveDistance)//判定到达
            {
                // Debug.Log("lift arrive a floor");
                currentFloor = midTargetFloor;//到达了某一层
                IntGamingSave gamingSave;
                if (TryGetComponent(out gamingSave) && !gamingSave.ban)
                {
                    gamingSave.saveGamingData(midTargetFloor);
                }

                if (midTargetFloor == targetFloor)//到达目的层
                {
                    rigid.MovePosition(new Vector3(transform.position.x, transform.position.y - distance, transform.position.z));
                    //严格对齐地面  如果玩家的碰撞体是椭圆可以不严格对齐 ，也能行走       
                    rigid.velocity = Vector2.zero;
                    if (playerIsOnLift)
                    {
                        playerRigid.velocity = new Vector2(playerRigid.velocity.x, 0);
                        playerRigid.MovePosition(new Vector2(playerRigid.transform.position.x, playerRigid.transform.position.y - distance));
                        //Debug.Log("stop");
                    }
                }
                else//继续移动 更新楼层
                {
                    if (rigid.velocity.y > 0)
                        moveUp();
                    else moveDown();
                }

            }
        }
    }
    void FixedUpdate()
    {

    }



    /// <summary>
    /// 电梯开关控制电梯的接口函数
    /// </summary>
    public void setTargetFloor(int floor)//调用时已经保证floor一定合法且不等于currentfloor
    {

        targetFloor = floor;

        float distance = floor - currentFloor;
        float moveSpeed;
        if (distance > 0)
        {
            moveSpeed = speed;
            moveUp();
        }
        else
        {
            moveSpeed = -speed;
            moveDown();
        }

        rigid.velocity = new Vector2(0, moveSpeed);
        if (playerIsOnLift)
        {
            playerRigid.velocity = new Vector2(playerRigid.velocity.x, moveSpeed);
            //Debug.Log("with");
        }

    }    //目前对一次攻击打到多个机关未做处理 以最后受击的机关为目标

    public void moveUp()//向上一层
    {
        midTargetFloor = (int)Mathf.Floor(currentFloor) + 1;//向下取整后+1 表示上一层 
        currentFloor = midTargetFloor - 0.5f;//表示正在向mid层运动
        midFloorHeight = gears[midTargetFloor - 1].floorHeight;//对应楼层的地面位置
    }

    public void moveDown()
    {
        midTargetFloor = (int)Mathf.Ceil(currentFloor) - 1;//向上取整后-1 表示下一层 
        currentFloor = midTargetFloor + 0.5f;//表示正在向mid层运动
        midFloorHeight = gears[midTargetFloor - 1].floorHeight;//对应楼层的地面位置
    }
}

