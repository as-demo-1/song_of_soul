using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbController : MonoBehaviour
{
    // 默认值 方便迁移到配置文件
    public int ConfigGravity = 5;
    public int ConfigClimbSpeed = 60;
    public float ConfigCheckRadius = 0.3f;

    private bool m_isClimb;
    private Rigidbody2D m_rb;

    private Transform m_ropeCheck;
    private LayerMask m_ropeLayer;

    // Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_ropeLayer = LayerMask.GetMask("Rope");
        m_ropeCheck = transform.Find("RopeCheck");
    }

    // Update is called once per frame
    void Update()
    {
        // 判断是否在绳子上
        if (Physics2D.OverlapCircle(m_ropeCheck.position, ConfigCheckRadius, m_ropeLayer))
        {
            // 按下上键开始攀爬
            if (Input.GetKey(KeyCode.UpArrow) ||
                Input.GetKey(KeyCode.W))
            {
                OnClimb();
            }

            // 下键或跳跃则取消攀爬
            if (Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.S) ||
                Input.GetKeyDown(KeyCode.Z))
            {
                OnUnclimb();
            }
        }
        // 否则取消攀爬
        else
        {
            OnUnclimb();
        }
    }

    private void OnClimb ()
    {
        // 攀爬时取消角色受力
        m_rb.velocity = Vector3.zero;

        // 如果攀爬中，则位置匀速上移
        if (m_isClimb)
        {
            Vector2 pos = transform.position;
            pos += (ConfigClimbSpeed * Vector2.up * Time.deltaTime);
            m_rb.MovePosition(pos);
        }
        // 切换到攀爬状态 将刚体重力置为0
        else
        {
            m_rb.gravityScale = 0;
            m_isClimb = true;
        }
    }

    private void OnUnclimb()
    {
        // 取消攀爬状态 恢复重力
        if (m_isClimb)
        {
            m_rb.gravityScale = ConfigGravity;
            m_isClimb = false;
        }
    }
}
