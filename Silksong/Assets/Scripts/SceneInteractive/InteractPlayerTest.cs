using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public struct MoveEventAction
{
    public Vector3 pos1;
    public Vector3 pos2;
    public float time;
    public float step;
    public UnityAction action;

    public MoveEventAction(Vector3 p1, Vector3 p2, float s, UnityAction a)
    {
        pos1 = p1;
        pos2 = p2;
        step = s;
        time = s;
        action = a;
    }
}

public class InteractPlayerTest : MonoBehaviour
{
    public static InteractPlayerTest Instance;

    public Queue<MoveEventAction> PlayerKeyActions = new Queue<MoveEventAction>();
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (InteractManager.Instance.CollidingObject)
            {
                InteractManager.Instance.CollidingObject?.GetComponent<InteractController>().InactItem.Interact();
            }
        }

        if (PlayerKeyActions.Count != 0)
        {
            MoveEventAction moe = PlayerKeyActions.Dequeue();
            moe.time -= 1;
            if (moe.time > -0.00001f)
            {
                transform.position = Vector3.Lerp(moe.pos2, moe.pos1, moe.time / moe.step);
                PlayerKeyActions.Enqueue(moe);
            }
            else
            {
                moe.action();
            }
        }
    }
}
