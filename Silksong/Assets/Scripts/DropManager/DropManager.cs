using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropManager
{
    public List<DropInfo> GetDrop4Monster(string monsterID)
    {
        MonsterInfor monster = MonsterInfoManager.Instance.GetOneMonsterByID(monsterID);

        List<DropInfo> ret = new List<DropInfo>();

        if (monster.Drop1 == "0")
        {
            return ret;
        }

        if (monster.Drop1.Substring(0, 1) == "1")
        {
            ret.Add(new DropInfo(ResSystem.Instance.GetOneItemByID(monster.Drop1), monster.Drop1Num));
        }
        else if (monster.Drop1.Substring(0, 1) == "3")
        {
            ret.Add(new DropInfo(ItemTableSystem.Instance.GetOneItemByID(monster.Drop1), monster.Drop1Num));
        }

        if (monster.Drop2 == "0")
        {
            return ret;
        }

        if (monster.Drop2.Substring(0, 1) == "1")
        {
            ret.Add(new DropInfo(ResSystem.Instance.GetOneItemByID(monster.Drop2), monster.Drop2Num));
        }
        else if (monster.Drop1.Substring(0, 1) == "3")
        {
            ret.Add(new DropInfo(ItemTableSystem.Instance.GetOneItemByID(monster.Drop2), monster.Drop2Num));
        }

        if (monster.Drop3 == "0")
        {
            return ret;
        }

        if (monster.Drop3.Substring(0, 1) == "1")
        {
            ret.Add(new DropInfo(ResSystem.Instance.GetOneItemByID(monster.Drop3), monster.Drop3Num));
        }
        else if (monster.Drop1.Substring(0, 1) == "3")
        {
            ret.Add(new DropInfo(ItemTableSystem.Instance.GetOneItemByID(monster.Drop3), monster.Drop3Num));
        }

        return ret;
    }

    public List<DropInfo> GetDrop4Inact(InactDrop inactDrop)
    {
        List<DropInfo> ret = new List<DropInfo>();

        if (inactDrop.Drop1 == "0")
        {
            return ret;
        }

        if (inactDrop.Drop1.Substring(0, 1) == "1")
        {
            ret.Add(new DropInfo(ResSystem.Instance.GetOneItemByID(inactDrop.Drop1), inactDrop.Drop1Num));
        }
        else if (inactDrop.Drop1.Substring(0, 1) == "3")
        {
            ret.Add(new DropInfo(ItemTableSystem.Instance.GetOneItemByID(inactDrop.Drop1), inactDrop.Drop1Num));
        }

        if (inactDrop.Drop2 == "0")
        {
            return ret;
        }

        if (inactDrop.Drop2.Substring(0, 1) == "1")
        {
            ret.Add(new DropInfo(ResSystem.Instance.GetOneItemByID(inactDrop.Drop2), inactDrop.Drop2Num));
        }
        else if (inactDrop.Drop1.Substring(0, 1) == "3")
        {
            ret.Add(new DropInfo(ItemTableSystem.Instance.GetOneItemByID(inactDrop.Drop2), inactDrop.Drop2Num));
        }

        if (inactDrop.Drop3 == "0")
        {
            return ret;
        }

        if (inactDrop.Drop3.Substring(0, 1) == "1")
        {
            ret.Add(new DropInfo(ResSystem.Instance.GetOneItemByID(inactDrop.Drop3), inactDrop.Drop3Num));
        }
        else if (inactDrop.Drop1.Substring(0, 1) == "3")
        {
            ret.Add(new DropInfo(ItemTableSystem.Instance.GetOneItemByID(inactDrop.Drop3), inactDrop.Drop3Num));
        }

        return ret;
    }

    private static DropManager _instance;
    public static DropManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DropManager();
            }

            return _instance;
        }
    }
}

public struct DropInfo
{
    public ItemInfo info;
    public int dropNum;
    public DropInfo (ItemInfo _info, int _dropNum)
    {
        info = _info;
        dropNum = _dropNum;
    }
}
