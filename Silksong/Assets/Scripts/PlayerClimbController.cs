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
        if (Physics2D.OverlapCircle(m_ropeCheck.position, ConfigCheckRadius, m_ropeLayer))
        {
            if (Input.GetKey(KeyCode.UpArrow) ||
                Input.GetKey(KeyCode.W))
            {
                OnClimb();
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.S) ||
                Input.GetKeyDown(KeyCode.Z))
            {
                OnUnClimb();
            }
        }
        else
        {
            OnUnClimb();
        }
    }

    private void OnClimb ()
    {
        m_rb.velocity = Vector3.zero;

        if (m_isClimb)
        {
            Vector2 pos = transform.position;
            pos += (ConfigClimbSpeed * Vector2.up * Time.deltaTime);
            m_rb.MovePosition(pos);
        }
        else
        {
            m_rb.gravityScale = 0;
            m_isClimb = true;
        }
    }

    private void OnUnClimb()
    {
        if (m_isClimb)
        {
            m_rb.gravityScale = ConfigGravity;
            m_isClimb = false;
        }
    }
}
