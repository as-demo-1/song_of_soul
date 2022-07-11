using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    //确定碰撞获取
    private BoxCollider2D boxCollider2D;
    //确定吸引范围
    private CircleCollider2D circleCollider2D;
    private Rigidbody2D rigidbody2D;

    private float speed = 20f;//吸取时飞行速度
    private bool isAttracted = false;//是否被目标吸引
    private Vector3 targetPosition;//目标位置
    private GameObject targetGameObject;//目标对象
    private float jumpForce = 300f;

    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private LayerMask groundLayerMask;

    public int bounceCount;//弹跳次数 unity编辑可见

    [SerializeField] private int m_BounceCount;
    [SerializeField] private float colliderRadius;//吸取的碰撞盒大小


    float launchTime = 1.0f;
    bool launch = false;

    private int m_MoneyNum = 0;       //代表的钱币数量
    


    void OnEnable()
    {
        targetGameObject = GameObject.FindWithTag("Player");
    }

    // Start is called before the first frame update
    void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        circleCollider2D = GetComponent<CircleCollider2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        circleCollider2D.radius = colliderRadius;
        m_BounceCount = bounceCount;
    }
     

    private void FixedUpdate()
    {
        LaunchCoin();
        Bounce();
        attract();
    }

    //吸引用球状碰撞体
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (circleCollider2D.IsTouchingLayers(playerLayerMask))//待定
        {
            isAttracted = true;
            rigidbody2D.gravityScale = 0;
        }
    }
  
    ///获得用方形碰撞体
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (boxCollider2D.IsTouchingLayers(playerLayerMask))
        {
            RecycleCoin();
            //TODO:玩家增加金钱,应该在数据类中派发事件
            EventManager.Instance.Dispatch<int>(EventType.onMoneyChange,m_MoneyNum);

        }
    }

    //发射金币
    void LaunchCoin() 
    {
        if (!launch) {
            float _randomx = Random.Range(-1.0f,1.0f);
            float _randomy = Random.Range(1.0f, 1.5f);
            Vector2 dir = new Vector2(_randomx, _randomy);
            rigidbody2D.AddForce(dir * jumpForce);
            launch = true;
        }
    }

    /// <summary>
    /// 金币弹跳
    /// </summary>
    private void Bounce()
    {
        if (IsGround() && !isAttracted && m_BounceCount >= 0)
        {
            if (rigidbody2D.velocity.x != 0)    //消除发射时带来的惯性
            {
                rigidbody2D.velocity = Vector2.zero;
            }
            rigidbody2D.AddForce(Vector2.up * jumpForce);
            m_BounceCount -= 1;
        }
    }
    private bool IsGround()
    {
        return boxCollider2D.IsTouchingLayers(groundLayerMask);
    }

    /// <summary>
    /// 金币被吸引
    /// </summary>
    private void attract()
    {
        if (isAttracted)
        {
            GetTargetPos();
            transform.Translate((-transform.position + targetPosition) * Time.fixedDeltaTime * speed, Space.World);
        }
    }

    private void GetTargetPos() 
    {
        targetPosition = targetGameObject.transform.position;
    }

    /// <summary>
    /// 回收金币预制
    /// </summary>
    private void RecycleCoin() 
    {
        isAttracted = false;
        m_BounceCount = bounceCount;
        rigidbody2D.gravityScale = 3;
        launch = false;
        CoinGenerator.Instance.RecycleCoinsPrefabs(this.gameObject);
    }

    /// <summary>
    /// 设置金币的钱数
    /// </summary>
    /// <param name="moneyNum"></param>
    public void SetCoinMoneyNum(int moneyNum) 
    {
        m_MoneyNum = moneyNum;
    }

}
