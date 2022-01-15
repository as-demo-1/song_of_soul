using UnityEngine;

public enum itemInventoryType
{
    Consumables,
    Equipments,
    Default
}
public enum ItemInventoryActionType
{
    Use,
    Equip,
    DoNothing
}

[CreateAssetMenu(fileName = "ItemType", menuName = "Inventory/ItemType")]
public class ItemTypeSO : ScriptableObject
{
    [Tooltip("The action associated with the item type")]
    [SerializeField]
    private string _actionName = default;

    [Tooltip("The action associated with the item type")]
    [SerializeField]
    private string _typeName = default;

    [Tooltip("The Item's background color in the UI")]
    [SerializeField] private Color _typeColor = default;
    [Tooltip("The Item's type")]
    [SerializeField] private itemInventoryType _type = default;

    [Tooltip("The Item's action type")]
    [SerializeField] private ItemInventoryActionType _actionType = default;


    [Tooltip("The tab type under which the item will be added")]
    [SerializeField] private InventoryTabSO _tabType = default;

    public string ActionName => _actionName;
    public string TypeName => _typeName;
    public Color TypeColor => _typeColor;
    public ItemInventoryActionType ActionType => _actionType;
    public itemInventoryType Type => _type;
    public InventoryTabSO TabType => _tabType;
}