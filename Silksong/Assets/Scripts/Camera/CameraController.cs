using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    private static CameraController _instance;
    public static CameraController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CameraController();
            }

            return _instance;
        }
    }

    [Header("这个是主摄像机")]
    public CinemachineVirtualCamera mMainVirtualCamera;
    [Header("这个是用于切换的摄像机")]
    public CinemachineVirtualCamera mSecondVirtualCamera;
    public CinemachineVirtualCamera mThirdVirtualCamera;
    public Trigger2DHidePath mCurHidePath = null;
    private CinemachineVirtualCamera mCurVCamera
    {
        get
        {
            if (mCurHidePath == null)
            {
                return mMainVirtualCamera;
            }
            else
            {
                return mSecondVirtualCamera;
            }
        }
    }

    [Header("这个是UI摄像机")]
    public Camera mStageCamera;

    void Awake()
    {
        print(gameObject.name);
        if (_instance != null)
        {
            GameObject bound = transform.Find("Boundary").gameObject;
            GameObject BoundNext = Instantiate(bound);
            BoundNext.name = "MainCameraBoundary";
            BoundNext.transform.position = gameObject.transform.position;
            BoundNext.transform.rotation = gameObject.transform.rotation;
            BoundNext.transform.localScale = gameObject.transform.lossyScale;
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

  
    public void AfterChangeScene()
    {
        GameObject mainCameraBoundary = GameObject.Find("MainCameraBoundary");
        if (mainCameraBoundary == null)
        {
            Debug.LogWarning("未找到MainCameraBoundary,需添加相应预设!");
            mainCameraBoundary = GameObject.Find("CameraPack/Boundary");
        }
        PolygonCollider2D polygon = mainCameraBoundary.GetComponent<PolygonCollider2D>();
        CinemachineConfiner confier = mMainVirtualCamera.GetComponent<CinemachineConfiner>();
        confier.m_BoundingShape2D = polygon;
        
        GameObject tempCam = GameObject.Find("TempCamera");
        if (tempCam != null)
        {
            GameObject.Destroy(tempCam);
        }
    }

    public void BeforeChangeScene()
    {
        if (mCurHidePath != null)
        {
            mCurHidePath.BeforeChangeScene();
        }
    }

    public void BeforeEnterBound(Trigger2DHidePath trigger2DHidePath)
    {
        if (mCurHidePath != null)
        {
            mCurHidePath.ForceExitEvent();
        }
        mCurHidePath = trigger2DHidePath;
    }

    public void BeforeExitBound(Trigger2DHidePath trigger2DHidePath)
    {
        if (mCurHidePath == trigger2DHidePath)
        {
            mCurHidePath = null;
        }
    }

    private bool usingOtherCam;
    public void OnGetTargetCam()
    {

    }
}
