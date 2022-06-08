using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class EntanceTestItemTable : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        List<ItemInfo> items = ItemTableSystem.Instance.GetAllItems();
        Debug.Log("---获取全部道具---");
        foreach (var item in items)
        {
            Debug.Log(item.ID + " " + item.NameSid + " " + item.DescSid + " " + item.BuffID + " " + item.BuffVal);
        }
        Debug.Log("---获取id为30100001的道具---");
        ItemInfo one = ItemTableSystem.Instance.GetOneItemByID("30100001");
        Debug.Log(one.ID + " " + one.NameSid + " " + one.DescSid + " " + one.BuffID + " " + one.BuffVal);
        Debug.Log("---获取分类为01的道具---");
        items = ItemTableSystem.Instance.GetItemsByType("01");
        foreach (var item in items)
        {
            Debug.Log(item.ID + " " + item.NameSid + " " + item.DescSid + " " + item.BuffID + " " + item.BuffVal);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
