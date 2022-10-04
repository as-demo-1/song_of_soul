using UnityEngine;
using Cinemachine;
/// <summary>
/// 隐藏小路镜头切换的触发器
/// </summary>作者：次元
[RequireComponent(typeof(BoxCollider2D))]
public class Trigger2DHidePath : Trigger2DBase
{
    [SerializeField] private PolygonCollider2D boundary;
    [SerializeField] private Transform cameraPack;
    private GameObject vcam;

    private PolygonCollider2D preBoundary;
    private bool isSwitched;
    protected override void enterEvent()
    {
        Debug.Log("player 进入");
        if (vcam != null)
        {
            if (!isSwitched)
            {           
                vcam.GetComponent<CinemachineConfiner>().m_BoundingShape2D = boundary;
                isSwitched = true;
            }
            else
            {
                vcam.GetComponent<CinemachineConfiner>().m_BoundingShape2D = preBoundary;
                isSwitched = false;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
        vcam = cameraPack.Find("CM vcam1").gameObject;
        preBoundary = cameraPack.Find("Boundary").gameObject.GetComponent<PolygonCollider2D>();
        if (vcam == null)
        {
            Debug.Log("find vcam failed");
        }
        if (preBoundary == null)
        {
            Debug.Log("find boundary failed");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
