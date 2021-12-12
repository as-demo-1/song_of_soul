using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine.Utility;
using Cinemachine;
public class CinemachineCameraOffestExtend : CinemachineExtension
{
    [Tooltip("镜头与角色的偏移值")]
    public Vector3 offset;
    [Tooltip("上下看的间隔时间")]
    public float Interval = 2f;
    bool IfInEvent;
    float lookUpTimes = 0;
    float lookDownTimes = 0;
    float Dy;
    CinemachineFramingTransposer body;
    private void Start()
    {
        //EventsManager.Instance.
        Dy= Camera.main.transform.position.y - Camera.main.ScreenToWorldPoint(Vector3.zero).y;
        body= transform.GetChild(0).GetComponent<CinemachineFramingTransposer>();
        body.m_TrackedObjectOffset = offset;
    }
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        CinemachineFramingTransposer body = vcam.transform.GetChild(0).GetComponent<CinemachineFramingTransposer>();
        Vector3 m_Offset = body.m_TrackedObjectOffset;
        if (PlayerController.Instance && ((PlayerController.Instance.playerInfo.playerFacingRight && m_Offset.x < 0) ||
            (!PlayerController.Instance.playerInfo.playerFacingRight && m_Offset.x > 0)))
        {
            body.m_TrackedObjectOffset.x = -m_Offset.x;
        }
    }
    //public void OnEvent(Transform target)
    //{
    //    IfInEvent=true;
    //    offset = body.m_TrackedObjectOffset = target.position-GetComponent<CinemachineVirtualCameraBase>().Follow.transform.position;
    //}
    //public void OnEventOver()
    //{
    //    IfInEvent=false;
    //    body.m_TrackedObjectOffset = offset=baseOffset;
    //}
    private void Update()
    {
        LookUp(body, Time.deltaTime);
    }
    public void LookUp(CinemachineFramingTransposer body,float deltaTime)
    {
        if (IfInEvent) return;
        if (Input.GetAxisRaw("Vertical") > 0 && !Input.anyKeyDown)
        {
            lookUpTimes += deltaTime;
            if (lookUpTimes > Interval)
            {
                if (body.m_TrackedObjectOffset.y == offset.y)
                    body.m_TrackedObjectOffset.y += Dy;
            }
        }
        else if (Input.GetAxisRaw("Vertical") < 0 && !Input.anyKeyDown)
        {
            lookDownTimes += deltaTime;
            if (lookDownTimes > Interval)
            {
                if (body.m_TrackedObjectOffset.y == offset.y)
                {
                    body.m_TrackedObjectOffset.y -= Dy;
                }
            }
        }
        else
        {
            lookUpTimes = 0;
            lookDownTimes = 0;
            body.m_TrackedObjectOffset.y=offset.y;
        }
    }
}
