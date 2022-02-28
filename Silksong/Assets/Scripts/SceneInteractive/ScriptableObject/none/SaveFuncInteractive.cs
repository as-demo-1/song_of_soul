using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "save", menuName = "Interactive/func/save")]
public class SaveFuncInteractive : FuncInteractiveBaseSO
{
    [SerializeField]
    private EFuncInteractItemType _funcInteractItemType = EFuncInteractItemType.SAVE;

    public override EFuncInteractItemType FuncInteractItemType => _funcInteractItemType;

    protected override GameObject InitChild(InteractLoad load)
    {
        GameObject go = base.InitChild(load);
        SaveController save = go.AddComponent<SaveController>();
        save.SaveSystem = load.SaveSystem;
        return go;
    }

    public override void DoAction()
    {
        InteractManager.Instance.InteractObject.GetComponent<SaveController>()
            .SaveSystem.SaveDataToDisk();
    }
}
