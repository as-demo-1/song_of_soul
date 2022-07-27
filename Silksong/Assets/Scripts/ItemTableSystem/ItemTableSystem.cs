using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ItemTableSystem
{
    // 1. 读取excel进行预处理
    // 2. 保存到so文件
    // 3. 游戏启动时读取so文件并保存到内存
    // 4. 将内存的数据通过接口提供相应的功能

    // 存放所有数据的字典
    private Dictionary<string, ItemInfo> _itemDic = new Dictionary<string, ItemInfo>();
    // 每种类别的item的字典
    private Dictionary<string, List<ItemInfo>> _typeDic = new Dictionary<string, List<ItemInfo>>();
    // 存放所有数据的列表
    private List<ItemInfo> _itemList = new List<ItemInfo>();

    private ItemTableSystem()
    {
        string path = "ScriptableObjects/items";
        var all = Resources.LoadAll<ItemTableSO>(path);

        foreach (var item in all)
        {
            _itemDic[item.ID] = new ItemInfo(item.ID, item.NameSid, item.DescSid, item.BuffID, item.BuffVal, item.Icon, item.ItemSO);
            _itemList.Add(_itemDic[item.ID]);

            if (!_typeDic.ContainsKey(item.TypeID))
            {
                _typeDic[item.TypeID] = new List<ItemInfo>();
            }

            _typeDic[item.TypeID].Add(_itemDic[item.ID]);
        }
    }

    // 初始化，将所有道具加载到内存
    public void Init()
    {
    }


    // 获取全部道具
    public List<ItemInfo> GetAllItems()
    {
        return _itemList;
    }

    // 获取获取某一类别的道具
    public List<ItemInfo> GetItemsByType(string TypeID)
    {
        if (_typeDic.ContainsKey(TypeID))
        {
            return _typeDic[TypeID];
        }
        else
        {
            Debug.LogError("分类为" + TypeID + "的道具类型不存在，请检查道具表以及输入TypeID");
            return default;
        }
    }

    // 获取某一种道具
    public ItemInfo GetOneItemByID(string id)
    {
        if (_itemDic.ContainsKey(id))
        {
            return _itemDic[id];
        }
        else
        {
            Debug.LogError("id为" + id + "的道具不存在，请检查道具表以及输入id");
            return default;
        }
    }

    private static ItemTableSystem _instance;
    public static ItemTableSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ItemTableSystem();
            }

            return _instance;
        }
    }
}

public class ItemInfo
{
    private string _id;                 // 物品id    id
    private string _nameSid;            // 物品名称  item name
    private string _descSid;            // 物品说明  item desc
    private string _buffId;             // 效果id    the id of buff
    private string _buffVal;            // 效果数值  buff effect
    private Sprite _icon = default;     // 图标     icon asset
    private ItemSO _itemSO = default;   // 物品so   itemso

    public string ID => _id;
    public string NameSid => _nameSid;
    public string DescSid => _descSid;
    public string BuffID => _buffId;
    public string BuffVal => _buffVal;
    public Sprite Icon => _icon;
    public ItemSO ItemSO => _itemSO;

    public ItemInfo(string id, string nameSid, string descSid, string buffId, string buffVal, Sprite icon, ItemSO itemSO)
    {
        _id = id;
        _nameSid = nameSid;
        _descSid = descSid;
        _buffId = buffId;
        _buffVal = buffVal;
        _icon = icon;
        _itemSO = itemSO;
    }
}
