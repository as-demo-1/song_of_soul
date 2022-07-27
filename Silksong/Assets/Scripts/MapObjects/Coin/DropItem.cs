using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour
{
    //ȷ����ײ��ȡ
    private BoxCollider2D boxCollider2D;
    //ȷ��������Χ
    private CircleCollider2D circleCollider2D;
    private Rigidbody2D rigidbody2D;

    private float speed = 20f;//��ȡʱ�����ٶ�
    private bool isAttracted = false;//�Ƿ�Ŀ������
    private Vector3 targetPosition;//Ŀ��λ��
    private GameObject targetGameObject;//Ŀ�����
    private float jumpForce = 300f;

    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private LayerMask groundLayerMask;

    public int bounceCount;//�������� unity�༭�ɼ�

    [SerializeField] private int m_BounceCount;
    [SerializeField] private float colliderRadius;//��ȡ����ײ�д�С


    float launchTime = 1.0f;
    bool launch = false;

    private int m_ItemNum = 0;       //�����Ǯ������
    private ItemInfo m_Item = default;



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

    //��������״��ײ��
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (circleCollider2D.IsTouchingLayers(playerLayerMask))//����
        {
            isAttracted = true;
            rigidbody2D.gravityScale = 0;
        }
    }
  
    ///����÷�����ײ��
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (boxCollider2D.IsTouchingLayers(playerLayerMask))
        {
            RecycleCoin();
            //TODO:������ӽ�Ǯ,Ӧ�������������ɷ��¼�
            EventManager.Instance.Dispatch<ItemInfo, int>(EventType.onItemChange, m_Item, m_ItemNum);
        }
    }

    //������
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
    /// ��ҵ���
    /// </summary>
    private void Bounce()
    {
        if (IsGround() && !isAttracted && m_BounceCount >= 0)
        {
            if (rigidbody2D.velocity.x != 0)    //��������ʱ�����Ĺ���
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
    /// ��ұ�����
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
    /// ���ս��Ԥ��
    /// </summary>
    private void RecycleCoin() 
    {
        isAttracted = false;
        m_BounceCount = bounceCount;
        rigidbody2D.gravityScale = 3;
        launch = false;
        ItemGenerator.Instance.RecycleCoinsPrefabs(this.gameObject);
    }

    /// <summary>
    /// ���ý�ҵ�Ǯ��
    /// </summary>
    /// <param name="dropInfo"></param>
    public void SetDropInfo(DropInfo dropInfo) 
    {
        m_Item = dropInfo.info;
        m_ItemNum = dropInfo.dropNum;
    }
}
