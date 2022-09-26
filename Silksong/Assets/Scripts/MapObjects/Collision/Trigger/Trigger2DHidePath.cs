using UnityEngine;
using Cinemachine;
/// <summary>
/// 隐藏小路镜头切换的触发器
/// </summary>作者：次元
[RequireComponent(typeof(BoxCollider2D))]
public class Trigger2DHidePath : Trigger2DBase
{
    [SerializeField] private PolygonCollider2D boundary;
<<<<<<< Updated upstream
    [SerializeField] private Transform cameraPack;
    private GameObject vcam;
    public CinemachineVirtualCameraBase switchToCam;
=======
    [SerializeField] private CameraPack cameraPack;
>>>>>>> Stashed changes

    private bool atRightSide;
    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (cameraPack != null)
        {
<<<<<<< Updated upstream
            if (!isSwitched)
            {
                //vcam.GetComponent<CinemachineConfiner>().m_BoundingShape2D = boundary;
                switchToCam.gameObject.SetActive(true);
                isSwitched = true;
            }
            else
            {
                //vcam.GetComponent<CinemachineConfiner>().m_BoundingShape2D = preBoundary;
                switchToCam.gameObject.SetActive(false);
                isSwitched = false;
=======
            if (SameSide(collision.transform))
            {
                cameraPack.SetBoundary(boundary);
            }
            else
            {
                cameraPack.SetBoundary(cameraPack.Boundary);
>>>>>>> Stashed changes
            }
        }
    }
    protected override void exitEvent()
    {
        
    }

    // Use this for initialization
    void Start()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
<<<<<<< Updated upstream
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
        switchToCam.gameObject.SetActive(false);
=======
        atRightSide = (boundary.transform.position.x - transform.position.x) > 0 ? true : false;
>>>>>>> Stashed changes
    }

    bool SameSide(Transform _transform)
    {
        bool result = false;
        if (((_transform.position.x - transform.position.x) > 0) == atRightSide)
        {
            result = true;
        }

        return result;
    }
}
