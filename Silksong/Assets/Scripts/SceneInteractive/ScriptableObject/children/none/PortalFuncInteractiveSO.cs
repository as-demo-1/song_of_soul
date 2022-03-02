using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "portal", menuName = "Interactive/func/portal")]
public class PortalFuncInteractiveSO : FuncMultiInteractiveBaseSO
{
    [SerializeField]
    [HideInInspector]
    private EFuncInteractItemType _funcInteractItemType = EFuncInteractItemType.PORTAL;

    public override EFuncInteractItemType FuncInteractItemType => _funcInteractItemType;

    protected override void SetField(GameObject go, InteractiveUnitBaseSO interactiveUnit, int index)
    {
        base.SetField(go, interactiveUnit, index);

        PortalController portal = go.AddComponent<PortalController>();
        portal.index = index;
    }

    protected override void DoInteract()
    {
        PortalController portal = InteractManager.Instance.GetInteractiveItemComponent<PortalController>();

        Vector3 pos = GetOtherPortal(portal.index).Position;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = pos;
        base.DoInteract();
    }

    private InteractiveUnitBaseSO GetOtherPortal(int index)
    {
        if (index == 1)
        {
            return InteractiveUnitList[0];
        }
        else if (index == 0)
        {
            return InteractiveUnitList[1];
        }
        else
        {
            throw new UnityException("参数错误");
        }
    }

    protected override void SetPositionChild(GameObject go, Vector3 pos)
    {
        int index = go.GetComponent<PortalController>().index;

        InteractiveUnitList[index].Position = pos;
    }
}
