using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEditor;


public class CameraVolumeManager : MonoSingleton<CameraVolumeManager>
{
    [Header("修改CameraPack.Prefab时需要修改的变量")] 
    [SerializeField] private string _camPackName = "CameraPack";
    [SerializeField] private string _cam1Name = "CM vcam1";
    [SerializeField] private string _cam2Name = "CM vcam2";
    [SerializeField] private string _cam3Name = "CM vcam3";


    //摄像机父物体
    private Transform cameraPack;

    //三个摄像机
    private Transform _mainVCam;
    private Transform _vcam2;
    private Transform _vcam3;

    //对应摄像机的CMVCam类
    private CinemachineVirtualCamera _mainVcamCM;
    private CinemachineVirtualCamera _vcam2CM;

    private CinemachineVirtualCamera _vcam3CM;

    //CinemachineConfiner
    private CinemachineConfiner _vcam2CMConfiner;
    private CinemachineConfiner _vcam3CMConfiner;


    //设置三个摄像机显示比例的类
    private CinemachineMixingCamera _MixingCamera;


    //
    private Tween cam1WeightSwitchTween = null;
    private Tween cam2WeightSwitchTween = null;
    private Tween cam3WeightSwitchTween = null;

    private Tween orthographicSizeSwitchTween = null;

    private Dictionary<Transform, bool> isOnUseList;
    private List<Trigger2DHidePath> inVolumeList;

    public int HighestPriorityPos
    {
        get
        {
            int max = -1;
            int curPriority = Int32.MinValue;
            for (var i = 0; i < inVolumeList.Count; i++)
            {
                if (inVolumeList[i].priority > curPriority)
                {
                    max = i;
                    curPriority = inVolumeList[i].priority;
                }
            }
            if(max == -1) Debug.LogError("inVolumeList 为空");
            return max;
        }
    }

    private float target0 = 0.001f;
    private float target1 = 1;
    public void Start()
    {
        cameraPack = GameObject.Find(_camPackName).transform;
        if (cameraPack == null)
        {
            Debug.LogError("没找到CameraPack!");
            return;
        }

        _MixingCamera = cameraPack.GetComponent<CinemachineMixingCamera>();

        //主摄像机
        _mainVCam = cameraPack.Find(_cam1Name);
        if (_mainVCam == null)
        {
            Debug.LogError("没找到摄像机1");
            return;
        }

        _mainVcamCM = _mainVCam.GetComponent<CinemachineVirtualCamera>();
        //摄像机2
        _vcam2 = cameraPack.Find(_cam2Name);
        if (_vcam2 == null)
        {
            Debug.LogError("没找到摄像机2");
            return;
        }

        _vcam2CM = _vcam2.GetComponent<CinemachineVirtualCamera>();
        //摄像机3
        _vcam3 = cameraPack.Find(_cam3Name);
        if (_vcam3 == null)
        {
            Debug.LogError("没找到摄像机3");
            return;
        }

        _vcam3CM = _vcam3.GetComponent<CinemachineVirtualCamera>();
        //Confiner，设置Boundary
        _vcam2CMConfiner = _vcam2.GetComponent<CinemachineConfiner>();
        _vcam3CMConfiner = _vcam3.GetComponent<CinemachineConfiner>();

        isOnUseList = new Dictionary<Transform, bool>();
        isOnUseList.Add(_mainVCam,false);
        isOnUseList.Add(_vcam2,false);
        isOnUseList.Add(_vcam3,false);

        inVolumeList = new List<Trigger2DHidePath>();
    }

    public void TriggerEnterVolume(Transform triggerTransform)
    {
        Trigger2DHidePath trig2D = triggerTransform.GetComponent<Trigger2DHidePath>();
        //当前没有在任何体积内
        if (inVolumeList.Count == 0)
        {
            EnterVolume(triggerTransform);
            inVolumeList.Add(trig2D);
            return;
        }
        // 如果新的优先级比较高
        if (inVolumeList[HighestPriorityPos].priority < trig2D.priority)
        {
            //TODO:从A进入B
            Debug.Log("从低优先级进入高优先级");
        }
        //添加进List
        inVolumeList.Add(trig2D);
        
    }
    //TODO: 这个记得让HidePath调用
    public void TriggerExitVolume(Transform triggerTransform)
    {
        Trigger2DHidePath trig2D = triggerTransform.GetComponent<Trigger2DHidePath>();
        //当前没有在任何体积内
        if (inVolumeList.Count == 0 || !inVolumeList.Contains(trig2D))
        {
            Debug.LogError("当前List为空或者找不到改容器，没有可以退出的容器");
            return;
        }
        inVolumeList.Remove(trig2D);

        if (inVolumeList.Count != 0)
        {
            //TODO: 从当前容器到最高的Priority容器
            Debug.Log("体积A换到体积B");
        }
        else
        {
            //TODO: 没东西了，直接跳回主镜头
            ExitVolume(triggerTransform);
        }

    }
    
    
    

    private void EnterVolume(Transform triggerTransform)
    {
        if (_vcam2CMConfiner == null || _vcam3CMConfiner == null)
        {
            Debug.LogError("寻找不到摄像机CinemachineConfiner组件");
            return;
        }

        Trigger2DHidePath triggerHidePath = triggerTransform.GetComponent<Trigger2DHidePath>();
        //cameraController.BeforeEnterBound(this);
        Debug.Log($"进入相机区域 {triggerTransform.name}");

        //是否看向前方
        if (triggerHidePath.isLookForward)
        {
            Transform cameraCheckComponents;
            cameraCheckComponents = PlayerController.Instance.transform.Find("CameraCheckComponents");
            if (cameraCheckComponents == null)
            {
                Debug.LogError("找不到CameraPack的CameraCheckComponents");
                return;
            }

            Transform lookAtPoint = cameraCheckComponents.Find("CameraLookAtPoint");
            if (lookAtPoint == null)
            {
                Debug.LogError("找不到CameraPack的lookAtPoint");
                return;
            }

            _vcam2CM.Follow = lookAtPoint.transform;
        }


        //给2号摄像机设置Boundry
        _vcam2CMConfiner.m_BoundingShape2D = triggerHidePath.boundary;
        //将1,2,3摄像机比重从 1,0,0设置为0,1,0
        cam1WeightSwitchTween = DOTween.To(
            () => _MixingCamera.GetWeight(0),
            x => _MixingCamera.SetWeight(0, x),
            target0, triggerHidePath.switchTime);
        cam2WeightSwitchTween = DOTween.To(
            () => _MixingCamera.GetWeight(1),
            x => _MixingCamera.SetWeight(1, x),
            target1, triggerHidePath.switchTime);
        cam3WeightSwitchTween = DOTween.To(
            () => _MixingCamera.GetWeight(2),
            x => _MixingCamera.SetWeight(2, x),
            target0, triggerHidePath.switchTime);
        orthographicSizeSwitchTween = DOTween.To(
            () => _vcam2CM.m_Lens.OrthographicSize,
            x => _vcam2CM.m_Lens.OrthographicSize = x,
            triggerHidePath.orthographicSize, triggerHidePath.switchTime);
        CinemachineFramingTransposer composer =
            _vcam2CM.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (triggerHidePath.lockCameraX)
        {
            composer.m_DeadZoneWidth = 2;
            composer.m_SoftZoneWidth = 2;
        }
        else
        {
            composer.m_DeadZoneWidth = 0.2f;
            composer.m_SoftZoneWidth = 0.8f;
        }

        if (triggerHidePath.lockCameraY)
        {
            composer.m_DeadZoneHeight = 2;
            composer.m_SoftZoneHeight = 2;
        }
        else
        {
            composer.m_DeadZoneHeight = 0.2f;
            composer.m_SoftZoneHeight = 0.8f;
        }

        //isSwitched = true;
        isOnUseList[_mainVCam] = false;
        isOnUseList[_vcam2] = true;
        isOnUseList[_vcam3] = false;
    }


    public void ExitVolume(Transform triggerTransform)
    {

        if (_vcam2CMConfiner == null || _vcam3CMConfiner == null)
        {
            Debug.LogError("寻找不到摄像机CinemachineConfiner组件");
            return;
        }
        Trigger2DHidePath triggerHidePath = triggerTransform.GetComponent<Trigger2DHidePath>();

        // cameraController.BeforeExitBound(this);
        Debug.Log($"进入相机区域 {triggerTransform.name}");
        
        CinemachineFramingTransposer composer =
            _vcam2CM.GetCinemachineComponent<CinemachineFramingTransposer>();
        composer.m_DeadZoneWidth = 0.2f;
        composer.m_SoftZoneWidth = 0.8f;
        composer.m_DeadZoneHeight = 0.2f;
        composer.m_SoftZoneHeight = 0.8f;

        cam1WeightSwitchTween = DOTween.To(
            () => _MixingCamera.GetWeight(0),
            x => _MixingCamera.SetWeight(0, x),
            target1, triggerHidePath.switchTime);
        cam2WeightSwitchTween = DOTween.To(
            () => _MixingCamera.GetWeight(1),
            x => _MixingCamera.SetWeight(1, x),
            target0, triggerHidePath.switchTime);
        cam3WeightSwitchTween = DOTween.To(
            () => _MixingCamera.GetWeight(2),
            x => _MixingCamera.SetWeight(2, x),
            target0, triggerHidePath.switchTime);

        
        isOnUseList[_mainVCam] = true;
        isOnUseList[_vcam2] = false;
        isOnUseList[_vcam3] = false;
    }
}