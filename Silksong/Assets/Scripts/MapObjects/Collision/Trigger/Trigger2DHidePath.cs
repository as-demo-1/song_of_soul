using UnityEngine;
using Cinemachine;
/// <summary>
/// 隐藏小路镜头切换的触发器
/// </summary>作者：次元
[RequireComponent(typeof(BoxCollider2D))]
public class Trigger2DHidePath : Trigger2DBase
{
    [SerializeField] private PolygonCollider2D boundary;
    [SerializeField] private CameraPack cameraPack;

    private bool atRightSide;
    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (cameraPack != null)
        {
            if (SameSide(collision.transform))
            {
                cameraPack.SetBoundary(boundary);
            }
            else
            {
                cameraPack.SetBoundary(cameraPack.Boundary);
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
        atRightSide = (boundary.transform.position.x - transform.position.x) > 0 ? true : false;
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
