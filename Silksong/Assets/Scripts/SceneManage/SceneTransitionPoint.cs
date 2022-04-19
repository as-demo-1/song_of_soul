using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionPoint : Trigger2DBase
{
    public string newSceneName;
    public SceneEntrance.EntranceTag entranceTag;
    public bool resetInputValuesOnTransition = true;

    [SerializeField] private SaveSystem _saveSystem;

    protected override void enterEvent()
    {        
        _saveSystem.SaveDataToDisk();
        SceneController.TransitionToScene(this);
    }
}
