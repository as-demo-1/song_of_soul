using UnityEngine;
using Cinemachine;
using DG.Tweening;

// todo 修改相机镜头时，如果此时已经有镜头了，那么执行一次exit
/// <summary>
/// 隐藏小路镜头切换的触发器 作者：次元
/// 修改：将镜头改为 CM MixingCamera 通过调整权重进行平滑切换 by 敢敢
/// </summary>
public class Trigger2DHidePath : Trigger2DBase
{
    [Header("相机切换后的Bound, 是子物体, 建议弄大点, 而自身挂载的Box是触发器"), SerializeField]
    private PolygonCollider2D boundary;
    [Header("建议运行状态下直接修改相机 CM vcam_other , 然后在编辑状态把值复制过来")]
    [Header("相机切换后大小"), SerializeField]
    private int mOrthographicSize = 4;
    [Header("切换时间"), SerializeField]
    private float mSwitchTime = 1;
    [Header("锁轴有极限,人物不能超出2倍,如果想更多,调整bound试试")]
    [Header("此处相机锁左右移动, 如果想锁主相机, 则将CM vcam1/Body/Dead Zone Width拉到最大"), SerializeField]
    private bool mLockCameraX = false;
    [Header("锁轴有极限,人物不能超出2倍,如果想更多,调整bound试试")]
    [Header("此处相机锁上下移动, 如果想锁主相机, 则将CM vcam1/Body/Dead Zone Height拉到最大"), SerializeField]
    private bool mLockCameraY = false;

    private Transform cameraPack;
    private CameraController cameraController;

    private CinemachineVirtualCamera mMainCinemachineVirtualCamera;

    private GameObject vcam;
    private CinemachineConfiner mCinemachineConfiner;
    private CinemachineVirtualCamera mCinemachineVirtualCamera;

    private Tween mSwitchTween0 = null;
    private Tween mSwitchTween1 = null;
    private Tween mmSizeTween0 = null;

    private CinemachineMixingCamera mMixingCamera;

    private PolygonCollider2D preBoundary;
    private bool isSwitched;
    private bool isChangeScene = false;

    void Start()
    {
        preBoundary = GetComponent<PolygonCollider2D>();
        cameraPack = GameObject.Find("CameraPack").transform;
        if (cameraPack == null)
        {
            Debug.LogError("没找到CameraPack!");
            return;
        }
        mMixingCamera = cameraPack.GetComponent<CinemachineMixingCamera>();
        cameraController = cameraPack.GetComponent<CameraController>();
        mMainCinemachineVirtualCamera = cameraPack.Find("CM vcam1").gameObject.GetComponent<CinemachineVirtualCamera>();

        vcam = cameraPack.Find("CM vcam_other").gameObject;
        if (vcam == null)
        {
            Debug.LogError("find vcam failed");
            return;
        }
        mCinemachineConfiner = vcam.GetComponent<CinemachineConfiner>();
        mCinemachineVirtualCamera = vcam.GetComponent<CinemachineVirtualCamera>();
    }

    protected override void enterEvent()
    {
        if (mCinemachineConfiner != null && !isSwitched)
        {
            cameraController.BeforeEnterBound(this);
            print($"进入相机区域 {transform.name}");
            _KillTween();
            float target0 = 0.01f;
            float target1 = 1;
            mCinemachineConfiner.m_BoundingShape2D = boundary;
            mSwitchTween0 = DOTween.To(
                () => mMixingCamera.GetWeight(0),
                x => mMixingCamera.SetWeight(0, x),
                target0, mSwitchTime);
            mSwitchTween1 = DOTween.To(
                () => mMixingCamera.GetWeight(1),
                x => mMixingCamera.SetWeight(1, x),
                target1, mSwitchTime);
            mmSizeTween0 = DOTween.To(
                () => mCinemachineVirtualCamera.m_Lens.OrthographicSize, 
                x => mCinemachineVirtualCamera.m_Lens.OrthographicSize = x,
                mOrthographicSize, mSwitchTime);
            CinemachineFramingTransposer composer = mCinemachineVirtualCamera.
                GetCinemachineComponent<CinemachineFramingTransposer>();
            if (mLockCameraX)
            {
                composer.m_DeadZoneWidth = 2;
                composer.m_SoftZoneWidth = 2;
            }
            else
            {
                composer.m_DeadZoneWidth = 0.2f;
                composer.m_SoftZoneWidth = 0.8f;
            }
            if (mLockCameraY)
            {
                composer.m_DeadZoneHeight = 2;
                composer.m_SoftZoneHeight = 2;
            }
            else
            {
                composer.m_DeadZoneHeight = 0.2f;
                composer.m_SoftZoneHeight = 0.8f;
            }
            isSwitched = true;
        }
    }

    protected override void exitEvent()
    {
        _exitEvent(false);
    }

    private void _exitEvent(bool force)
    {
        if (isChangeScene)
        {
            return;
        }
        if (mCinemachineConfiner != null && isSwitched)
        {
            cameraController.BeforeExitBound(this);
            print($"退出相机区域 {transform.name} {force}");
            _KillTween();
            if (!force)
            {
                float target0 = 1;
                float target1 = 0.01f;
                CinemachineFramingTransposer composer = mCinemachineVirtualCamera.
                    GetCinemachineComponent<CinemachineFramingTransposer>();
                composer.m_DeadZoneWidth = 0.2f;
                composer.m_SoftZoneWidth = 0.8f;
                composer.m_DeadZoneHeight = 0.2f;
                composer.m_SoftZoneHeight = 0.8f;

                mSwitchTween0 = DOTween.To(() => mMixingCamera.GetWeight(0), x => mMixingCamera.SetWeight(0, x), target0, mSwitchTime);
                mSwitchTween1 = DOTween.To(() => mMixingCamera.GetWeight(1), x => mMixingCamera.SetWeight(1, x), target1, mSwitchTime);
                mSwitchTween1.onComplete = _ClearBoundary;
            }
            //else
            //{
            //    _ClearBoundary();
            //}
            isSwitched = false;
        }
    }

    public void BeforeChangeScene()
    {
        _exitEvent(true);
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

    private void _ClearBoundary()
    {
        if (mCinemachineConfiner != null && !isSwitched)
        {
            mCinemachineConfiner.m_BoundingShape2D = preBoundary;
        }
    }

    private void _KillTween()
    {
        if (mSwitchTween0 != null && mSwitchTween0.IsPlaying())
        {
            mSwitchTween0.Kill(false);
            mSwitchTween0 = null;
        }
        if (mSwitchTween1 != null && mSwitchTween1.IsPlaying())
        {
            mSwitchTween1.Kill(false);
            mSwitchTween1 = null;
        }
        if (mmSizeTween0 != null && mmSizeTween0.IsPlaying())
        {
            mmSizeTween0.Kill(false);
            mmSizeTween0 = null;
        }
    }
}
