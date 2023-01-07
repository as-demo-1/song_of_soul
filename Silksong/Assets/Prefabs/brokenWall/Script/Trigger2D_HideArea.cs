using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger2D_HideArea : Trigger2DBase
{
    public Animator animator;
    public List<Trigger2D_HideArea> connectArea;
    [SerializeField] private string _guid;
    [SerializeField] private SaveSystem _saveSystem;
    //public string GUID => ;
    //private void OnValidate()
    //{
    //    _guid=GUID;
    //}//
    protected void Start()
    {
        _guid = GetComponent<GuidComponent>().GetGuid().ToString();
        _saveSystem = GameManager.Instance.saveSystem;
        if (MapObjSaveSystem.ContainsObject(_guid)||_saveSystem.ContainDestructiblePlatformGUID(_guid))
        {
            Disappear();
        }
    }
    protected override void enterEvent()
    {
        base.enterEvent();
        Disappear();
    }
    public void Disappear()
    {
        MapObjSaveSystem.AddNewMapObject(_guid);
        animator.Play("hide");
        for(int i = 0; i < connectArea.Count; i++)
        {
            connectArea[i].DisappearSelf();
        }
    }
    public void DisappearSelf()
    {
        MapObjSaveSystem.AddNewMapObject(_guid);
        animator.Play("hide");
    }
}
