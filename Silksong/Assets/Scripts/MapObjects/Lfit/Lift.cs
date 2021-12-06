using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 电梯1.0
/// </summary>作者：青瓜
public class Lift : MonoBehaviour
{
    public int maxFloor;//从1开始计算
    public LiftFloorGear[] gears;//lift为该电梯的liftFloorGear在start时绑定到该数组  按从低层到高层顺序

    public float currentFloor;//当前层 若为x.5层则表示在x与x+1层之间
    public int targetFloor;//最终要到的层
    public int midTargetFloor;//现在层和目标层之间的中间层 用于在跨多层移动中记录电梯当前位置

    public float targetFloorHeight;//目标地面高度
    public float midFloorHeight;//中间层的地面高度

    public Transform liftFloorTransform;//电梯地板的位置 方便使电梯地板与地面高度一致

    public float speed;
    private float arriveDistance;//当电梯与目的地距离小于此值时 判定到达

    private PlayerController player;
    private Rigidbody2D playerRigid;

    private Rigidbody2D rigid;
    private BoxCollider2D floorCollider;//电梯地面的碰撞体

    private bool playerIsOnLift=false;//玩家是否在电梯上 用于同步速度
    void Awake()
    {
        gears = new LiftFloorGear[maxFloor];
        rigid = GetComponent<Rigidbody2D>();
        floorCollider = GetComponent<BoxCollider2D>();

    }
    void Start()
    {
        GameObject playerobj = GameObject.FindGameObjectWithTag("Player");
        if(player!=null)
        {
            player = playerobj.GetComponent<PlayerController>();
            playerRigid = player.GetComponent<Rigidbody2D>();
        }

        setFloorPosition();
        arriveDistance = speed * Time.fixedDeltaTime;
    }

    private void setFloorPosition()
    {
        float floorDistance= floorCollider.offset.y;
        floorDistance += (floorCollider.size.y / 2);
        floorDistance *= transform.lossyScale.y;
        liftFloorTransform.position = new Vector2(liftFloorTransform.position.x, transform.position.y + floorDistance);
    }

    void FixedUpdate()
    {
        if(rigid.velocity.y!=0)//电梯在移动
        {
            float distance = liftFloorTransform.position.y - midFloorHeight;
          //  Debug.Log(distance);
            if (Mathf.Abs(distance)< arriveDistance)//判定到达
            {
               // Debug.Log("lift arrive a floor");
                currentFloor = midTargetFloor;//到达了某一层
                
                if(midTargetFloor==targetFloor)//到达目的层
                {
                    //rigid.MovePosition(new Vector3(transform.position.x, transform.position.y - distance, transform.position.z));
                    //严格对齐地面  如果玩家的碰撞体是椭圆可以不严格对齐 ，也能行走       
                    rigid.velocity = Vector2.zero;
                    if (playerIsOnLift)
                    {
                        playerRigid.velocity = new Vector2(playerRigid.velocity.x, 0);
                        //Debug.Log("stop");
                    }
                }
                else//继续移动
                {
                    if (rigid.velocity.y > 0)
                        moveUp();
                    else moveDown();
                }

            }
        }
    }



    /// <summary>
    /// 电梯开关控制电梯的接口函数
    /// </summary>
    public void setTargetFloor(int floor)//调用时已经保证floor一定合法且不等于currentfloor
    {

        targetFloor = floor;
        targetFloorHeight = gears[floor - 1].floorHeight;

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

    }    //目前对一次攻击打到多个机关未做处理 以最后受击的机关目标

    public void moveUp()//向上一层
    {
        midTargetFloor = (int)Mathf.Floor(currentFloor)+1;//向下取整后+1 表示上一层 
        currentFloor = midTargetFloor - 0.5f;//表示正在向mid层运动
        midFloorHeight = gears[midTargetFloor - 1].floorHeight;//对应楼层的地面位置
    }

    public void moveDown()
    {
        midTargetFloor = (int)Mathf.Ceil(currentFloor) - 1;//向上取整后-1 表示下一层 
        currentFloor = midTargetFloor + 0.5f;//表示正在向mid层运动
        midFloorHeight = gears[midTargetFloor - 1].floorHeight;//对应楼层的地面位置
    }

    private void OnCollisionEnter2D(Collision2D collision)//也可以使用overlap来判断玩家是否在电梯上
    {
       /* if(collision.gameObject==player.gameObject && collision.otherCollider is BoxCollider2D)
        {
            playerIsOnLift = true;
        }*/
    }
    private void OnCollisionExit2D(Collision2D collision)
    {

       /* if (collision.gameObject == player.gameObject)
        {
            playerIsOnLift = false;
        }*/
    }
}
