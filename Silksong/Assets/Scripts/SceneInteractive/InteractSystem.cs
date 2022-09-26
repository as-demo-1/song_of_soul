using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractSystem
{
    // 1.每一个物体的prefab和so都由联系
    // 2.so文件3个层级，worldroot, container, item
    // 3.每个so文件都存在一个id作为唯一标识符
    // 4.每个item_so文件可以放置以下5种so子文件
    //   1.func
    //   2.dialog
    //   3.judge
    //   4.fulltext
    //   5.anmiation
    // 5.每个so子文件的生命周期为
    //   1.DoInteract
    //   2.Next
    // so文件使用next切换到下一阶段
    public void Next()
    {
        // 1.寻找当前周期
        // 2.获取当前物体
        // 3.获取当前物体的下一周期，如果存在，则调用
        // 4.如果不存在，则寻找item的下一个child
        // 5.如果无下一个child则调用Stop
    }

    public void Stop()
    {
    }

    Dictionary<int, InactWorldRootSO> _iwrd = new Dictionary<int, InactWorldRootSO>();
    Dictionary<int, InactContainerSO> _icd = new Dictionary<int, InactContainerSO>();
    Dictionary<int, InactItemSO> _iid = new Dictionary<int, InactItemSO>();

    int _iwrd_size = -1;
    int _icd_size = -1;
    int _iid_size = -1;

    public int SetIwrd(InactWorldRootSO iwr)
    {
        ++_iwrd_size;
        _iwrd[_iwrd_size] = iwr;
        return _iwrd_size;
    }

    public InactWorldRootSO GetIwrd(int iwr_id)
    {
        return _iwrd[iwr_id];
    }

    public int SetIcd(InactContainerSO ic)
    {
        ++_icd_size;
        _icd[_icd_size] = ic;
        return _icd_size;
    }

    public InactContainerSO GetIcd(int ic_id)
    {
        return _icd[ic_id];
    }

    public int SetIid(InactItemSO ii)
    {
        ++_iid_size;
        _iid[_iid_size] = ii;
        return _iid_size;
    }

    public InactItemSO GetIid(int ii_id)
    {
        return _iid[ii_id];
    }

    // ------- util -------

    public InactItemSO GetOtherIid42(int ii_id)
    {
        int ic_id = GetIid(ii_id).GetParent.Icid;
        if (GetIcd(ic_id).InactItemsSO.Count != 2)
        {
            return default;
        }
        if (GetIcd(ic_id).InactItemsSO[0].Iid == ii_id)
        {
            return GetIid(GetIcd(ic_id).InactItemsSO[1].Iid);
        }
        else
        {
            return GetIid(ii_id);
        }
    }

    public InactItemSO GetPlayerTarget(int ii_id)
    {
        int ic_id = GetIid(ii_id).GetParent.Icid;

        foreach(var item in GetIcd(ic_id).InactItemsSO)
        {
            if (item.InactItemType == EInact2.PLAYER_TARGET)
                return item;
        }

        return default;
    }

    public InactItemSO GetThisTarget(int ii_id)
    {
        int ic_id = GetIid(ii_id).GetParent.Icid;

        foreach (var item in GetIcd(ic_id).InactItemsSO)
        {
            if (item.InactItemType == EInact2.THIS_TARGET)
                return item;
        }

        return default;
    }

    private static InteractSystem _instance;
    public static InteractSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new InteractSystem();
            }

            return _instance;
        }
    }
}
