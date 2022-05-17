using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionPoint : Trigger2DBase
{
    public string newSceneName;
    public SceneEntrance.EntranceTag entranceTag;
    public bool resetInputValuesOnTransition = true;

    protected override void enterEvent()
    {
        SceneController.TransitionToScene(this);
    }
}
