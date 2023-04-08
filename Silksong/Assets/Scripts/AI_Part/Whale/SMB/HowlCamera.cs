using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowlCamera : StateMachineBehaviour
{
    public float cameraShakeForce;
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        CameraShakeManager.Instance.cameraShake(cameraShakeForce);
    }
}
