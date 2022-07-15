using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseTest : MonoBehaviour
{
    [SerializeField] public float chaseSpeed;
    [SerializeField] public bool isFaceWithSpeed;
    [SerializeField] public bool isFlying = false;
    [SerializeField] public bool lock_x_move = false;
    [SerializeField] public bool lock_y_move = false;
    [SerializeField] public bool isMoveWithCurve = false;
    [SerializeField] public float curveCycle;
    [SerializeField] public AnimationCurve curve;
    [SerializeField] public Enemy_FSM fSM_Manager;
    [SerializeField] private Vector3 v;

    private void Awake()
    {
        
    }

    private void FixedUpdate()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;
        v = fSM_Manager.getTargetDir(true);
        v = v.normalized;
        if (!isFlying)
        {
            if (v.x > 0)
                v = new Vector3(1, 0, 0);
            else
                v = new Vector3(-1, 0, 0);
        }
        if (lock_x_move)
            v.x = 0;
        if (lock_y_move)
            v.y = 0;
        if (isMoveWithCurve)
        {
            var vv = Vector3.Lerp(Vector3.zero, v, curve.Evaluate((Time.time / (curveCycle + 0.000001f)) % 1.0f));
            fSM_Manager.rigidbody2d.velocity = chaseSpeed * vv;
        }
        else
            fSM_Manager.rigidbody2d.velocity = chaseSpeed * v;
        if (isFaceWithSpeed)
            fSM_Manager.faceWithSpeed();
    }
}
