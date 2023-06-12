using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_GrappleRope : MonoBehaviour
{
    [Header("General Refernces:")]
    public Tutorial_GrapplingGun grapplingGun;
    public LineRenderer m_lineRenderer;

    [Header("General Settings:")]
    [SerializeField] private int percision = 40;//精确性 linerender点的数量 数量越多精确性越高
    [Range(0, 20)][SerializeField] private float straightenLineSpeed = 5;//从弯曲变直线的速度

    [Header("Rope Animation Settings:")]
    public AnimationCurve ropeAnimationCurve;
    [Range(0.01f, 4)][SerializeField] private float StartWaveSize = 2;//波浪的大小
    [HideInInspector]public float waveSize = 0;

    [Header("Rope Progression:")]//绳子前进
    public AnimationCurve ropeProgressionCurve;
    [SerializeField][Range(1, 50)] private float ropeProgressionSpeed = 1;

    float moveTime = 0;

    [HideInInspector] public bool isGrappling = true;

    bool strightLine = true;

    private void OnEnable()
    {
        moveTime = 0;
        isGrappling=false;
        m_lineRenderer.positionCount = percision;
        waveSize = StartWaveSize;
        strightLine = false;//目前是否直线(发射的时候是曲线，拉过去的时候是直线)
        LinePointsToFirePoint();//初始化所有的点
        m_lineRenderer.enabled = true;//打开lineRender
    }

    private void OnDisable()
    {
        m_lineRenderer.enabled = false;
        isGrappling = false;
    }
    private void LinePointsToFirePoint()
    {
        for (int i = 0; i < percision; i++)
        {
            m_lineRenderer.SetPosition(i, grapplingGun.firePoint.position);//把所有点都设置到发射点处
        }
    }

    private void Update()
    {
        moveTime += Time.deltaTime;//随时间增加的moveTime
        DrawRope();
    }

    void DrawRope()
    {
        if (!strightLine)//两个阶段 发射和拉过去
        {
            //如果最后一个点已经碰到抓到的点了，就拉过去
            //Debug.Log(Vector2.Distance(m_lineRenderer.GetPosition(percision), grapplingGun.grapplePoint));
            if (Vector2.Distance(m_lineRenderer.GetPosition(percision - 1),grapplingGun.grapplePoint)<1f)
            {
                strightLine = true;
            }
            else
            {
                DrawRopeWaves();//这里就是画波浪线的地方
            }
        }
        else
        {
            //Debug.Log("catch");
            if (!isGrappling)//抓到目标没
            {
                grapplingGun.Grapple();//这个函数的作用是吧目标拉过去
                isGrappling = true;
            }
            if (waveSize > 0)//波浪幅度逐渐变小
            {
                //Debug.Log(waveSize);
                waveSize -= Time.deltaTime * straightenLineSpeed;
                DrawRopeWaves();
            }
            else
            {
                //然后就可以直接开始画直线了
                waveSize = 0;

                if (m_lineRenderer.positionCount != 2) { m_lineRenderer.positionCount = 2; }

                DrawRopeNoWaves();
            }
        }
    }

    /// <summary>
    /// DrawRopeWaves才是重点，如何画出一条曲线
    /// </summary>
    void DrawRopeWaves()
    {
        for (int i = 0; i < percision; i++)//对于每个点画线
        {
            float delta = (float)i / ((float)percision - 1f);
            //前面一个是方向向量意思是向射击方向的垂直方向横向扩张成曲线*对应的值，这tm是个定值
            Vector2 offset = Vector2.Perpendicular(grapplingGun.grappleDistanceVector).normalized * ropeAnimationCurve.Evaluate(delta) * waveSize;
            //这个就是在射击方向上均匀分布的意思了
            Vector2 targetPosition = Vector2.Lerp(grapplingGun.firePoint.position, grapplingGun.grapplePoint, delta) + offset;
            //绳子的前进速度 这是一个速度渐行的过程
            Vector2 currentPosition = Vector2.Lerp(grapplingGun.firePoint.position, targetPosition, ropeProgressionCurve.Evaluate(moveTime) * ropeProgressionSpeed);
            m_lineRenderer.SetPosition(i, currentPosition);
        }
    }
    void DrawRopeNoWaves()
    {
        m_lineRenderer.SetPosition(0, grapplingGun.firePoint.position);
        m_lineRenderer.SetPosition(1, grapplingGun.grapplePoint);
    }
}
