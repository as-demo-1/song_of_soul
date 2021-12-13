using UnityEngine;

public enum InventoryTabType
{
    Default
}
[CreateAssetMenu(fileName = "InventoryTabType", menuName = "Inventory/Inventory Tab Type")]
public class InventoryTabSO : ScriptableObject
{
    [Tooltip("The tab Name that will be displayed in the inventory")]
    [SerializeField]
    private string _tabName = default;

    [Tooltip("The tab Picture that will be displayed in the inventory")]
    [SerializeField]
    private Sprite _tabIcon = default;


    [Tooltip("The tab type used to reference the item")]
    [SerializeField] private InventoryTabType _tabType = default;

    public string TabName => _tabName;
    public Sprite TabIcon => _tabIcon;
    public InventoryTabType TabType => _tabType;
}