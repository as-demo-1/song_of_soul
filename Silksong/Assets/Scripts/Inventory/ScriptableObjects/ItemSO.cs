using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Item", menuName = "Inventory/Item")]
public class ItemSO : SerializableScriptableObject
{
    [Tooltip("The name of the item")]
    [SerializeField] private string _id = default;

    [Tooltip("The name of the item")]
    [SerializeField] private string _name = default;

    [Tooltip("A preview image for the item")]
    [SerializeField]
    private Sprite _previewImage = default;

    [Tooltip("A description of the item")]
    [SerializeField]
    private string _description = default;

    //[Tooltip("The type of interaction")]
    //[SerializeField]
    //private EInteractiveItemType _interitemType = default;

    [Tooltip("The type of item")]
    [SerializeField]
    private ItemTypeSO _itemType = default;

    [Tooltip("A prefab reference for the model of the item")]
    [SerializeField]
    private GameObject _prefab = default;

    [Tooltip("Price of the item")]
    [SerializeField]
    private int _price = default;

    public string ID => _id;
    public string Name => _name;
    //public EInteractiveItemType m_itemType => _interitemType;
    public Sprite PreviewImage => _previewImage;
    public string Description => _description;
    public int price => _price;
    public ItemTypeSO ItemType => _itemType;
    public GameObject Prefab => _prefab;

    public void Crt(string id, string nameSid, string descSid, ItemTypeSO itemType)
    {
        _id = id;
        _name = nameSid;
        _description = descSid;
        _itemType = itemType;
    }

    public void Upd(string nameSid, string descSid)
    {
        _name = nameSid;
        _description = descSid;
    }
}
