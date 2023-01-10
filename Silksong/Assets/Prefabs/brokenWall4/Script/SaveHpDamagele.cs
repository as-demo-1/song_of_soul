using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveHpDamagele : HpDamable
{
    [SerializeField] private string _guid;
    [SerializeField] private SaveSystem _saveSystem;
    public bool ifSave;
    protected override void Awake()
    {
        base.Awake();
    }
    protected void Start()
    {
        _guid = GetComponent<GuidComponent>().GetGuid().ToString();
        _saveSystem = GameManager.Instance.saveSystem;
        if ((_saveSystem.ContainDestructiblePlatformGUID(_guid)
            ||MapObjSaveSystem.ContainsObject(_guid))&&ifSave)
        {
            Debug.Log("loadDie");
            die(null);
        }
    }
    protected override void die(DamagerBase damager)
    {
        base.die(damager);
        if (ifSave)
        {
            Debug.Log("±£´æguid");
            MapObjSaveSystem.AddNewMapObject(_guid);
        }
    }
}

