using System.IO;
using System.Data;
using ExcelDataReader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class ExcelLoad : MonoBehaviour
{
    public static Dictionary<int, string> Content = new Dictionary<int, string>(); //对话内容
    public static List<string> Condition = new List<string>();//存储条件
    public static Dictionary<int, List<string>> ConditionList = new Dictionary<int, List<string>>();//前面是对话ID（只要装每段对话的第一句对话的ID），后面是这个对话的条件列表
    public static Dictionary<int, List<int>> NPCID = new Dictionary<int, List<int>>(); //前面装NPCID，后面装这个NPCID的所有SID
    public static List<int> AllSID = new List<int>(); //给NPCID当工具人，装所有SID
    public static Dictionary<int, int> SidGetNpcID = new Dictionary<int, int>();//通过SID查找NPCID
    public static Dictionary<int, string> type = new Dictionary<int, string>();//前面是SID，后面是剧情对话类型
    public static Dictionary<int, string> NPCName = new Dictionary<int, string>(); //前面装NPC的SID，后面装NPCName
    public static Dictionary<int, List<SortedDictionary<int, DialogueSO>>> npcDialogueDic = new Dictionary<int, List<SortedDictionary<int, DialogueSO>>>();//把excel里面同一个NPCID的DialogueSO物体集合起来
    public static SortedDictionary<int, DialogueSO> dialoguesoDic = new SortedDictionary<int, DialogueSO>(); //前面是每个DialogueSO的StartID，用于按顺序装入SectionSO
    public static Dictionary<int, int> SIDList = new Dictionary<int, int>(); //前面储存SID，后面储存EID

    public static void ReadExcelStream(string FilePath)
    {
        FileStream stream = File.Open(FilePath, FileMode.Open, FileAccess.Read);
        IExcelDataReader excelDataReader = ExcelReaderFactory.CreateOpenXmlReader(stream);




        DataSet result = excelDataReader.AsDataSet();


        for (int TableNum = 0; TableNum < 2; TableNum++)
        {
            int columns = result.Tables[TableNum].Columns.Count;
            int rows = result.Tables[TableNum].Rows.Count;

            for (int i = 1; i < rows; i++) //第一行是属性名，从第二行开始
            {
                byte[] SIDLength = System.Text.Encoding.ASCII.GetBytes(result.Tables[TableNum].Rows[i][0].ToString());
                for (int j = 6; j < columns; j++)
                {
                    string nvalue = result.Tables[TableNum].Rows[i][j].ToString();
                    if (j == 6)
                    {
                        Content.Add(int.Parse(result.Tables[TableNum].Rows[i][1].ToString()), result.Tables[TableNum].Rows[i][3].ToString());//储存对话内容
                        if (SIDLength.Length != 0)
                        {
                            //AllSID.Add(int.Parse(result.Tables[0].Rows[i][0].ToString()));
                            NPCID[int.Parse(result.Tables[TableNum].Rows[i][4].ToString())] = new List<int>();
                            NPCID[int.Parse(result.Tables[TableNum].Rows[i][4].ToString())].Add(int.Parse(result.Tables[TableNum].Rows[i][0].ToString()));//储存对话的npcid
                            NPCName.Add(int.Parse(result.Tables[TableNum].Rows[i][0].ToString()), result.Tables[TableNum].Rows[i][6].ToString());
                            SIDList.Add(int.Parse(result.Tables[TableNum].Rows[i][0].ToString()), int.Parse(result.Tables[TableNum].Rows[i][2].ToString()));
                            SidGetNpcID.Add(int.Parse(result.Tables[TableNum].Rows[i][0].ToString()), int.Parse(result.Tables[TableNum].Rows[i][4].ToString()));
                            type.Add(int.Parse(result.Tables[TableNum].Rows[i][0].ToString()), result.Tables[TableNum].Rows[i][5].ToString());
                        }

                    }
                    if (j > 6)
                    {
                        byte[] array = System.Text.Encoding.ASCII.GetBytes(nvalue);
                        if (array.Length != 0)
                        {
                            Condition.Add(nvalue);
                            ConditionList.Add(int.Parse(result.Tables[TableNum].Rows[i][0].ToString()), Condition);
                        }
                    }
                    //Debug.Log(nvalue);
                }
                //Debug.Log(Content[int.Parse(result.Tables[0].Rows[i][0].ToString())]);
            }
        }


        //创建SO物体
        string CreatePath = Application.dataPath + "/Resources/SO_objects/";
        foreach (KeyValuePair<int, int> item in SIDList)
        {
            DialogueSO temp = DialogueSO.CreateSO(item.Key.ToString(), item.Value.ToString(), SidGetNpcID[item.Key], type[item.Key]);
            TalkSOManager.Instance.DialogueList.Add(temp);
            FileStream fs = new FileStream(CreatePath + item.Key.ToString() + "-" + item.Value.ToString() + ".asset", FileMode.Create);
            fs.Write(System.Text.Encoding.Default.GetBytes("%YAML 1.1\n"), 0, System.Text.Encoding.Default.GetBytes("%YAML 1.1\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("%TAG !u! tag:unity3d.com,2011:\n"), 0, System.Text.Encoding.Default.GetBytes("%TAG !u! tag:unity3d.com,2011:\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("--- !u!114 &11400000\n"), 0, System.Text.Encoding.Default.GetBytes("--- !u!114 &11400000\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("MonoBehaviour:\n"), 0, System.Text.Encoding.Default.GetBytes("MonoBehaviour:\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  m_ObjectHideFlags: 0\n"), 0, System.Text.Encoding.Default.GetBytes("  m_ObjectHideFlags: 0\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  m_CorrespondingSourceObject: {fileID: 0}\n"), 0, System.Text.Encoding.Default.GetBytes("  m_CorrespondingSourceObject: {fileID: 0}\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  m_PrefabInstance: {fileID: 0}\n"), 0, System.Text.Encoding.Default.GetBytes("  m_PrefabInstance: {fileID: 0}\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  m_PrefabAsset: {fileID: 0}\n"), 0, System.Text.Encoding.Default.GetBytes("  m_PrefabAsset: {fileID: 0}\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  m_GameObject: {fileID: 0}\n"), 0, System.Text.Encoding.Default.GetBytes("  m_GameObject: {fileID: 0}\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  m_Enabled: 1\n"), 0, System.Text.Encoding.Default.GetBytes("  m_Enabled: 1\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  m_EditorHideFlags: 0\n"), 0, System.Text.Encoding.Default.GetBytes("  m_EditorHideFlags: 0\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  m_Script: {fileID: 11500000, guid: 4177c01c752e641d2bc5b5134763bed5, type: 3}\n"), 0, System.Text.Encoding.Default.GetBytes("  m_Script: {fileID: 11500000, guid: 4177c01c752e641d2bc5b5134763bed5, type: 3}\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  m_Name: " + item.Key.ToString() + "-" + item.Value.ToString() + "\n"), 0, System.Text.Encoding.Default.GetBytes("  m_Name: " + item.Key.ToString() + "-" + item.Value.ToString() + "\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  m_EditorClassIdentifier: \n"), 0, System.Text.Encoding.Default.GetBytes("  m_EditorClassIdentifier: \n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  _startid: " + item.Key.ToString() + "\n"), 0, System.Text.Encoding.Default.GetBytes("  _startid: " + item.Key.ToString() + "\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  _endid: " + item.Value.ToString() + "\n"), 0, System.Text.Encoding.Default.GetBytes("  _endid: " + item.Value.ToString() + "\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  _content:\n"), 0, System.Text.Encoding.Default.GetBytes("  _content:\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  _npcid: " + SidGetNpcID[item.Key].ToString() + "\n"), 0, System.Text.Encoding.Default.GetBytes("  _npcid: " + SidGetNpcID[item.Key].ToString() + "\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  _type: " + type[item.Key] + "\n"), 0, System.Text.Encoding.Default.GetBytes("  _type: " + type[item.Key] + "\n").Length);
            fs.Write(System.Text.Encoding.Default.GetBytes("  _StatusList: \n"), 0, System.Text.Encoding.Default.GetBytes("  _StatusList: \n").Length);
            fs.Close();
        }

        //创建StatusSO物体
        string CreateStatusPath = Application.dataPath + "/Resources/SO_Status/";
        foreach (KeyValuePair<int, int> item in SIDList)
        {
            if (ConditionList.ContainsKey(item.Key))
            {
                foreach (string conditionName in ConditionList[item.Key])
                {
                    DialogueStatusSO temp = DialogueStatusSO.CreateStatusSO(conditionName, false);
                    TalkSOManager.Instance.DialogueStatusList.Add(temp);
                    FileStream fs = new FileStream(CreateStatusPath + conditionName + ".asset", FileMode.Create);
                    fs.Write(System.Text.Encoding.Default.GetBytes("%YAML 1.1\n"), 0, System.Text.Encoding.Default.GetBytes("%YAML 1.1\n").Length);
                    fs.Write(System.Text.Encoding.Default.GetBytes("%TAG !u! tag:unity3d.com,2011:\n"), 0, System.Text.Encoding.Default.GetBytes("%TAG !u! tag:unity3d.com,2011:\n").Length);
                    fs.Write(System.Text.Encoding.Default.GetBytes("--- !u!114 &11400000\n"), 0, System.Text.Encoding.Default.GetBytes("--- !u!114 &11400000\n").Length);
                    fs.Write(System.Text.Encoding.Default.GetBytes("MonoBehaviour:\n"), 0, System.Text.Encoding.Default.GetBytes("MonoBehaviour:\n").Length);
                    fs.Write(System.Text.Encoding.Default.GetBytes("  m_ObjectHideFlags: 0\n"), 0, System.Text.Encoding.Default.GetBytes("  m_ObjectHideFlags: 0\n").Length);
                    fs.Write(System.Text.Encoding.Default.GetBytes("  m_CorrespondingSourceObject: {fileID: 0}\n"), 0, System.Text.Encoding.Default.GetBytes("  m_CorrespondingSourceObject: {fileID: 0}\n").Length);
                    fs.Write(System.Text.Encoding.Default.GetBytes("  m_PrefabInstance: {fileID: 0}\n"), 0, System.Text.Encoding.Default.GetBytes("  m_PrefabInstance: {fileID: 0}\n").Length);
                    fs.Write(System.Text.Encoding.Default.GetBytes("  m_PrefabAsset: {fileID: 0}\n"), 0, System.Text.Encoding.Default.GetBytes("  m_PrefabAsset: {fileID: 0}\n").Length);
                    fs.Write(System.Text.Encoding.Default.GetBytes("  m_GameObject: {fileID: 0}\n"), 0, System.Text.Encoding.Default.GetBytes("  m_GameObject: {fileID: 0}\n").Length);
                    fs.Write(System.Text.Encoding.Default.GetBytes("  m_Enabled: 1\n"), 0, System.Text.Encoding.Default.GetBytes("  m_Enabled: 1\n").Length);
                    fs.Write(System.Text.Encoding.Default.GetBytes("  m_EditorHideFlags: 0\n"), 0, System.Text.Encoding.Default.GetBytes("  m_EditorHideFlags: 0\n").Length);
                    fs.Write(System.Text.Encoding.Default.GetBytes("  m_Script: {fileID: 11500000, guid: 3f6c80dad5aa9437c8364b51de178ef1, type: 3}\n"), 0, System.Text.Encoding.Default.GetBytes("  m_Script: {fileID: 11500000, guid: 3f6c80dad5aa9437c8364b51de178ef1, type: 3}\n").Length);
                    fs.Write(System.Text.Encoding.Default.GetBytes("  m_Name: " + conditionName + "\n"), 0, System.Text.Encoding.Default.GetBytes("  m_Name: " + conditionName + "\n").Length);
                    fs.Write(System.Text.Encoding.Default.GetBytes("  m_EditorClassIdentifier: \n"), 0, System.Text.Encoding.Default.GetBytes("  m_EditorClassIdentifier: \n").Length);
                    fs.Write(System.Text.Encoding.Default.GetBytes("  _conditionname: " + conditionName + "\n"), 0, System.Text.Encoding.Default.GetBytes("  _conditionname: " + conditionName + "\n").Length);
                    fs.Write(System.Text.Encoding.Default.GetBytes("  _judge: " + "0" + "\n"), 0, System.Text.Encoding.Default.GetBytes("  _judge: " + "0" + "\n").Length);
                    fs.Close();
                }
            }
        }

        //创建SectionSO物体
        string CreateSectionPath = Application.dataPath + "/Resources/SO_Section/";
        foreach (int item in NPCID.Keys)
        {
            if (!Directory.Exists(CreateSectionPath + NPCName[NPCID[item][0]] + "   .asset"))
            {
                DialogueSectionSO temp = DialogueSectionSO.CreateSectionSO(NPCName[NPCID[item][0]], item);
                TalkSOManager.Instance.DialogueSectionList.Add(temp);
                FileStream fs = new FileStream(CreateSectionPath + NPCName[NPCID[item][0]] + ".asset", FileMode.Create);
                fs.Write(System.Text.Encoding.Default.GetBytes("%YAML 1.1\n"), 0, System.Text.Encoding.Default.GetBytes("%YAML 1.1\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("%TAG !u! tag:unity3d.com,2011:\n"), 0, System.Text.Encoding.Default.GetBytes("%TAG !u! tag:unity3d.com,2011:\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("--- !u!114 &11400000\n"), 0, System.Text.Encoding.Default.GetBytes("--- !u!114 &11400000\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("MonoBehaviour:\n"), 0, System.Text.Encoding.Default.GetBytes("MonoBehaviour:\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("  m_ObjectHideFlags: 0\n"), 0, System.Text.Encoding.Default.GetBytes("  m_ObjectHideFlags: 0\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("  m_CorrespondingSourceObject: {fileID: 0}\n"), 0, System.Text.Encoding.Default.GetBytes("  m_CorrespondingSourceObject: {fileID: 0}\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("  m_PrefabInstance: {fileID: 0}\n"), 0, System.Text.Encoding.Default.GetBytes("  m_PrefabInstance: {fileID: 0}\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("  m_PrefabAsset: {fileID: 0}\n"), 0, System.Text.Encoding.Default.GetBytes("  m_PrefabAsset: {fileID: 0}\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("  m_GameObject: {fileID: 0}\n"), 0, System.Text.Encoding.Default.GetBytes("  m_GameObject: {fileID: 0}\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("  m_Enabled: 1\n"), 0, System.Text.Encoding.Default.GetBytes("  m_Enabled: 1\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("  m_EditorHideFlags: 0\n"), 0, System.Text.Encoding.Default.GetBytes("  m_EditorHideFlags: 0\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("  m_Script: {fileID: 11500000, guid: 967a888e75317034a8f7b5cc8d5c48bc, type: 3}\n"), 0, System.Text.Encoding.Default.GetBytes("  m_Script: {fileID: 11500000, guid: 967a888e75317034a8f7b5cc8d5c48bc, type: 3}\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("  m_Name: " + NPCName[NPCID[item][0]] + "\n"), 0, System.Text.Encoding.Default.GetBytes("  m_Name: " + NPCName[NPCID[item][0]] + "\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("  m_EditorClassIdentifier: \n"), 0, System.Text.Encoding.Default.GetBytes("  m_EditorClassIdentifier: \n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("  _dialogueList:\n"), 0, System.Text.Encoding.Default.GetBytes("  _dialogueList:\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("  _dialogueStatusList:\n"), 0, System.Text.Encoding.Default.GetBytes("  _dialogueStatusList:\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("  _npcID: " + item.ToString() + "\n"), 0, System.Text.Encoding.Default.GetBytes("  _npcID: " + item.ToString() + "\n").Length);
                fs.Write(System.Text.Encoding.Default.GetBytes("  NPCName:" + NPCName[NPCID[item][0]] + "\n"), 0, System.Text.Encoding.Default.GetBytes("  NPCName:" + NPCName[NPCID[item][0]] + "\n").Length);
                fs.Close();
            }
        }


        excelDataReader.Close();
        Load();


    }
    public static void Load()
    {
        //把对话压入DialogueSO
        foreach (DialogueSO dialogueitem in TalkSOManager.Instance.DialogueList)
        {
            dialoguesoDic = new SortedDictionary<int, DialogueSO>();
            //Debug.Log(dialoguesoDic.Count);
            if (ConditionList.ContainsKey(dialogueitem.StartID))
            {
                foreach (string conditionname in ConditionList[dialogueitem.StartID])
                {
                    if (!dialogueitem.StatusList.Contains(conditionname))
                    {
                        dialogueitem.StatusList.Add(conditionname);
                    }
                }
            }
            if (dialogueitem.Content.Count == 0)
            {
                for (int id = dialogueitem.StartID; id < dialogueitem.EndID + 1; id++)
                {
                    //Debug.Log(id);
                    dialogueitem.Content.Add(Content[id]);
                }
            }
            //对话装进去后用每段对话的第一句ID来标识对话
            dialoguesoDic.Add(dialogueitem.StartID, dialogueitem);

            if (!npcDialogueDic.ContainsKey(dialogueitem.NPCID))
            {
                npcDialogueDic[dialogueitem.NPCID] = new List<SortedDictionary<int, DialogueSO>>();
                npcDialogueDic[dialogueitem.NPCID].Add(dialoguesoDic);
            }
            else
            {
                npcDialogueDic[dialogueitem.NPCID].Add(dialoguesoDic);
            }
        }



        //把DialogueSO和DialogueStatusSO压入SectionSO
        //把属于这个角色的DialogueSO装入这个角色的SectionSO
        //把DialogueSO和DialogueStatusSO压入SectionSO
        foreach (DialogueSectionSO sectionitem in TalkSOManager.Instance.DialogueSectionList)
        {
            sectionitem.DialogueList = new List<DialogueSO>();
            sectionitem.DialogueStatusList = new List<DialogueStatusSO>();
            foreach (SortedDictionary<int, DialogueSO> dic in npcDialogueDic[sectionitem.NPCID])//用SID定位
            {
                //把属于这个角色的DialogueSO装入这个角色的SectionSO
                foreach (KeyValuePair<int, DialogueSO> item in dic) //key是StartID
                {
                    /*Debug.Log(sectionitem.NPCID);
                    Debug.Log(item.Value);
                    Debug.Log(sectionitem.DialogueList.Count);*/
                    if (!sectionitem.DialogueList.Contains(item.Value))
                    {
                        sectionitem.DialogueList.Add(item.Value);
                    }

                    //把控制这个角色的对话的所有条件装入SectionSO
                    if (ConditionList.ContainsKey(item.Key))
                    {
                        foreach (string conditionname in ConditionList[item.Key])
                        {
                            foreach (DialogueStatusSO statusitem in TalkSOManager.Instance.DialogueStatusList)
                            {
                                if (statusitem.ConditionName.Equals(conditionname) && !sectionitem.DialogueStatusList.Contains(statusitem))
                                {
                                    sectionitem.DialogueStatusList.Add(statusitem);
                                }
                            }
                        }
                    }
                }

            }
        }
    }
}
