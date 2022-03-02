using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "save", menuName = "Interactive/func/save")]
public class SaveFuncInteractiveSO : FuncSingleInteractiveBaseSO
{
    [SerializeField]
    [HideInInspector]
    private EFuncInteractItemType _funcInteractItemType = EFuncInteractItemType.SAVE;

    public override EFuncInteractItemType FuncInteractItemType => _funcInteractItemType;

    protected override GameObject InitChild(InteractLoad load)
    {
        GameObject go = base.InitChild(load);
        SaveController save = go.AddComponent<SaveController>();
        save.SaveSystem = load.SaveSystem;
        return go;
    }

    protected override void DoInteract()
    {
        InteractManager.Instance.GetInteractiveItemComponent<SaveController>()
            .SaveSystem.SaveDataToDisk();
        base.DoInteract();
    }
}
