using UnityEngine;
using Cinemachine;
using DG.Tweening;
using OfficeOpenXml.FormulaParsing.ExcelUtilities;
using UnityEditor;
using UnityEngine.Serialization;

// todo 修改相机镜头时，如果此时已经有镜头了，那么执行一次exit
/// <summary>
/// 隐藏小路镜头切换的触发器 作者：次元
/// 修改：将镜头改为 CM MixingCamera 通过调整权重进行平滑切换 by 敢敢
/// </summary>
public class Trigger2DHidePath : Trigger2DBase
{
    [Header("相机切换后的Bound, 是子物体, 建议弄大点, 而自身挂载的Box是触发器")]
    public PolygonCollider2D boundary;
    [Header("建议运行状态下直接修改相机 CM vcam_other , 然后在编辑状态把值复制过来")]
    [Header("相机切换后大小")]
    public int orthographicSize = 4;
    [Header("切换时间")]
    public float switchTime = 1;
    [Header("锁轴有极限,人物不能超出2倍,如果想更多,调整bound试试")]
    [Header("此处相机锁左右移动, 如果想锁主相机, 则将CM vcam1/Body/Dead Zone Width拉到最大")]
    public bool lockCameraX = false;
    [Header("锁轴有极限,人物不能超出2倍,如果想更多,调整bound试试")]
    [Header("此处相机锁上下移动, 如果想锁主相机, 则将CM vcam1/Body/Dead Zone Height拉到最大")]
    public bool lockCameraY = false;

    [Header("是否让相机跟着角色前方（Player身上的LookAtPoint的position）")]
    public bool isLookForward = false;

    public Vector3 lookForwardVector;

    [Header("优先级，0为优先级最低，INT_MAX 为优先级最高")] 
    public int priority = 0;

    

    void Start()
    {
        // cameraPack = GameObject.Find("CameraPack").transform;
        // if (cameraPack == null)
        // {
        //     Debug.LogError("没找到CameraPack!");
        //     return;
        // }
        // mMixingCamera = cameraPack.GetComponent<CinemachineMixingCamera>();
        // cameraController = cameraPack.GetComponent<CameraController>();
        // mMainCinemachineVirtualCamera = cameraPack.Find("CM vcam1").gameObject.GetComponent<CinemachineVirtualCamera>();
        //
        // vcam = cameraPack.Find("CM vcam_other").gameObject;
        // if (vcam == null)
        // {
        //     Debug.LogError("find vcam failed");
        //     return;
        // }
        // mCinemachineConfiner = vcam.GetComponent<CinemachineConfiner>();
        // mCinemachineVirtualCamera = vcam.GetComponent<CinemachineVirtualCamera>();
        // preBoundary = cameraPack.Find("Boundary").GetComponent<PolygonCollider2D>();

    }

    protected override void enterEvent()
    {
         CameraVolumeManager.Instance.TriggerEnterVolume(this.transform);
    }
    
    protected override void exitEvent()
    {
        CameraVolumeManager.Instance.TriggerExitVolume(this.transform);
    }
    
    public void BeforeChangeScene()
    {
        // /_exitEvent(true);
        //isChangeScene = true;
        //if (mCinemachineConfiner != null)
        //{
        //    mCinemachineConfiner.m_BoundingShape2D = null;
        //}
    }

    public void ForceExitEvent()
    {
        //_exitEvent(true);
    }


}
