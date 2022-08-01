using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{

    [SerializeField] private float Distance = 100;

    public Transform firepoint;

    public LineRenderer m_lineRender;

    private Transform m_transform;
    // Start is called before the first frame update

    private void Awake()
    {
        m_transform = GetComponent<Transform>();
    }

    void FireLaser()
    {
        if (Physics2D.Raycast(m_transform.position, transform.right))
        {
            RaycastHit2D hit = Physics2D.Raycast(m_transform.position, transform.right);
            DrawRay(firepoint.position,hit.point);
        }
        else
        {
            DrawRay(firepoint.position,firepoint.transform.right * Distance);
        }
    }

    void DrawRay(Vector2 startPos, Vector2 endpos)
    {
        m_lineRender.SetPosition(0,startPos);
        m_lineRender.SetPosition(1,endpos);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FireLaser();
    }
}
