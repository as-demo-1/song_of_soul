using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeInteract : InteractTriggerBase<JudgeInteract>
{
    protected override void InteractEvent()
    {
        Debug.Log("判断框");
        Toggle("Judge", true);
        UIAddListener("Judge/Image/LButton", () => {
            Toggle("Judge", false);
            Debug.Log("yes");
            ContinueEvent();
        });
        UIAddListener("Judge/Image/RButton", () => {
            Toggle("Judge", false);
            Debug.Log("no");
            ContinueEvent();
        });
    }
}
