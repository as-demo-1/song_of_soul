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

    void Awake()
    {
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
    }

    public void BeforeChangeScene()
    {
        if (mCurHidePath != null)
        {
            mCurHidePath.BeforeChangeScene();
        }
    }
}
