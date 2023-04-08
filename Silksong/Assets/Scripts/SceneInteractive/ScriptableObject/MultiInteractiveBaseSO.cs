using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultiInteractiveBaseSO : InteractiveBaseSO
{
    [Tooltip("The list of the interactiveUnit")]
    [SerializeField] private List<InteractiveUnitBaseSO> _interactive_unit_list = default;

    public List<InteractiveUnitBaseSO> InteractiveUnitList => _interactive_unit_list;

    protected virtual void SetField(GameObject go, InteractiveUnitBaseSO interactiveUnit, int index)
    {
        go.transform.position = interactiveUnit.Position;

        SpriteRenderer interactiveObjectSprite = go.GetComponent<SpriteRenderer>();
        InteractiveObjectTrigger interactiveObjectTrigger = go.AddComponent<InteractiveObjectTrigger>();

        interactiveObjectSprite.sprite = interactiveUnit.Icon;
        interactiveObjectSprite.flipX = !interactiveUnit.IsFaceRight;
        interactiveObjectTrigger.InteractiveItem = this;

        UIComponentManager.Instance.SetText(go, InteractConstant.UITipText, interactiveUnit.Content);
    }

    protected override GameObject InitChild(InteractLoad load)
    {
        GameObject parent = Instantiate(new GameObject("parent"), load.transform);

        for (int i = 0; i < InteractiveUnitList.Count; i++)
        {
            GameObject childGo = Instantiate(load.ItemPrefab, parent.transform);
            SetField(childGo, InteractiveUnitList[i], i);
        }

        return parent;
    }
}
