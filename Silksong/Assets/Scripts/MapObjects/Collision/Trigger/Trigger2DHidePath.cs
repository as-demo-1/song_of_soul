using UnityEngine;
using Cinemachine;
/// <summary>
/// 隐藏小路镜头切换的触发器
/// </summary>作者：次元
[RequireComponent(typeof(BoxCollider2D))]
public class Trigger2DHidePath : Trigger2DBase
{
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private Transform boundary;
    [SerializeField] private CameraPack cameraPack;

    private bool atLeft;
    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (cameraPack != null && collision.CompareTag("Player"))
        {
            Debug.Log(CheckSide(collision.transform));
            if (CheckSide(collision.transform))
            {
                cameraPack.ChangeVcam(vcam);
            }
            else
            {
                cameraPack.ChangeVcam(cameraPack.vcam);

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
        atLeft = boundary.transform.localPosition.x < 0 ? true : false;
    }

    bool CheckSide(Transform _transform)
    {
        bool result = false;
        Debug.Log(_transform.position.x - transform.position.x);
        if (((_transform.position.x - transform.position.x) < 0) == atLeft)
        {
            result = true;
        }

        return result;
    }
}
