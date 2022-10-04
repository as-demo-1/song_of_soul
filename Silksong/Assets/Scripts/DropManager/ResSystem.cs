using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResSystem
{
    // 存放所有数据的字典
    private Dictionary<string, ItemInfo> _itemDic = new Dictionary<string, ItemInfo>();
    // 存放所有数据的列表
    private List<ItemInfo> _itemList = new List<ItemInfo>();

    private ResSystem()
    {
        string path = "ScriptableObjects/res";
        var all = Resources.LoadAll<ResScriptableObject>(path);

        foreach (var item in all)
        {
            _itemDic[item.ID] = new ItemInfo(item.ID, item.NameSid, item.DescSid, "", "", default, default);
            _itemList.Add(_itemDic[item.ID]);
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

    private static ResSystem _instance;
    public static ResSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ResSystem();
            }

            return _instance;
        }
    }
}
