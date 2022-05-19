using System.IO;
using System.Data;
using ExcelDataReader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using UnityEditor;

public class ExcelLoad
{
    public static Dictionary<int, string> Content = new Dictionary<int, string>(); //�Ի�����
    public static Dictionary<int, List<string>> ConditionList = new Dictionary<int, List<string>>();//前面是对话StartID，后面是控制对话的条件
    public static List<string> Condition = new List<string>();//�洢����
    public static Dictionary<int, List<int>> NPCID = new Dictionary<int, List<int>>(); //ǰ��װNPCID������װ���NPCID������SID
    public static List<int> AllSID = new List<int>(); //��NPCID�������ˣ�װ����SID
    public static Dictionary<int, int> SidGetNpcID = new Dictionary<int, int>();//ͨ��SID����NPCID
    public static Dictionary<int, string> type = new Dictionary<int, string>();//ǰ����SID�������Ǿ���Ի�����
    public static Dictionary<int, string> NPCName = new Dictionary<int, string>(); //ǰ��װNPC��SID������װNPCName
    public static Dictionary<int, List<SortedDictionary<int, DialogueSO>>> npcDialogueDic = new Dictionary<int, List<SortedDictionary<int, DialogueSO>>>();//��excel����ͬһ��NPCID��DialogueSO���弯������
    public static SortedDictionary<int, DialogueSO> dialoguesoDic = new SortedDictionary<int, DialogueSO>(); //ǰ����ÿ��DialogueSO��StartID�����ڰ�˳��װ��SectionSO
    public static Dictionary<int, int> SIDList = new Dictionary<int, int>(); //ǰ�洢��SID�����洢��EID

    private static ExcelLoad _instance;
    public static ExcelLoad Instance => _instance;


    //[MenuItem("Dialogue/Operation/ReadExcelStream", false, 1)]
    public static void ReadExcelStream()
    {
        string path = "Assets/Scripts/Dialogue/TalkSOManager.cs";
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        TalkSOManager temp = Resources.Load<TalkSOManager>(path);

        //Debug.Log("TalkSOManager");
        FileStream stream = File.Open("Assets/Resources/AllDialogue.xlsx", FileMode.Open, FileAccess.Read);
        IExcelDataReader excelDataReader = ExcelReaderFactory.CreateOpenXmlReader(stream);






        DataSet result = excelDataReader.AsDataSet();
        //Debug.Log("正常");


        for (int TableNum = 0; TableNum < 2; TableNum++)
        {
            int columns = result.Tables[TableNum].Columns.Count;
            int rows = result.Tables[TableNum].Rows.Count;

            for (int i = 1; i < rows; i++) //��һ�������������ӵڶ��п�ʼ
            {
                byte[] SIDLength = System.Text.Encoding.ASCII.GetBytes(result.Tables[TableNum].Rows[i][0].ToString());
                for (int j = 6; j < columns; j++)
                {
                    string nvalue = result.Tables[TableNum].Rows[i][j].ToString();
                    if (j == 6)
                    {
                        Content.Add(int.Parse(result.Tables[TableNum].Rows[i][1].ToString()), result.Tables[TableNum].Rows[i][3].ToString());//����Ի�����
                        if (SIDLength.Length != 0)
                        {
                            //AllSID.Add(int.Parse(result.Tables[0].Rows[i][0].ToString()));
                            NPCID[int.Parse(result.Tables[TableNum].Rows[i][4].ToString())] = new List<int>();
                            NPCID[int.Parse(result.Tables[TableNum].Rows[i][4].ToString())].Add(int.Parse(result.Tables[TableNum].Rows[i][0].ToString()));//����Ի���npcid
                            NPCName.Add(int.Parse(result.Tables[TableNum].Rows[i][0].ToString()), result.Tables[TableNum].Rows[i][6].ToString());
                            SIDList.Add(int.Parse(result.Tables[TableNum].Rows[i][0].ToString()), int.Parse(result.Tables[TableNum].Rows[i][2].ToString()));
                            SidGetNpcID.Add(int.Parse(result.Tables[TableNum].Rows[i][0].ToString()), int.Parse(result.Tables[TableNum].Rows[i][4].ToString()));
                            type.Add(int.Parse(result.Tables[TableNum].Rows[i][0].ToString()), result.Tables[TableNum].Rows[i][5].ToString());
                        }
                        //Debug.Log(int.Parse(result.Tables[TableNum].Rows[i][1].ToString()));

                    }
                    if (j > 6)
                    {
                        int re = 0;
                        byte[] array = System.Text.Encoding.ASCII.GetBytes(nvalue);

                        
                        if (array.Length != 0)
                        {
                            if (int.TryParse(result.Tables[TableNum].Rows[i][0].ToString(), out re))
                            {
                                if (!ConditionList.ContainsKey(int.Parse(result.Tables[TableNum].Rows[i][0].ToString())))
                                {
                                    ConditionList.Add(int.Parse(result.Tables[TableNum].Rows[i][0].ToString()), Condition);
                                }
                            }
                            //Condition.Add(nvalue);
                            ConditionList[int.Parse(result.Tables[TableNum].Rows[i][0].ToString())].Add(nvalue);
                            Debug.Log(ConditionList[int.Parse(result.Tables[TableNum].Rows[i][0].ToString())][0]);
                            Debug.Log(int.Parse(result.Tables[TableNum].Rows[i][0].ToString()));
                        }
                    }
                    //Debug.Log(nvalue);
                }
            }
        }
        //excelDataReader.Close();
        //Load();

    }

    //加载对话内容
    //[MenuItem("Dialogue/Operation/LoadExcel", false, 2)]
    public static void Load()
    {

        //�ѶԻ�ѹ��DialogueSO
        foreach (DialogueSO dialogueitem in TalkSOManager.Instance.DialogueListInstance)
        {
            dialogueitem.StatusList = new List<string>();
            dialogueitem.Content = new List<string>();
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
                    //Debug.Log(id + "-" +dialogueitem.StartID + "-" + dialogueitem.NPCID);
                    dialogueitem.Content.Add(Content[id]);
                }
            }
            //�Ի�װ��ȥ����ÿ�ζԻ��ĵ�һ��ID����ʶ�Ի�
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



        //��DialogueSO��DialogueStatusSOѹ��SectionSO
        //�����������ɫ��DialogueSOװ�������ɫ��SectionSO
        //��DialogueSO��DialogueStatusSOѹ��SectionSO
        foreach (DialogueSectionSO sectionitem in TalkSOManager.Instance.DialogueSectionListInstance)
        {
            sectionitem.DialogueList = new List<DialogueSO>();
            sectionitem.DialogueStatusList = new List<DialogueStatusSO>();
            //Debug.Log(sectionitem.NPCID);
            foreach (SortedDictionary<int, DialogueSO> dic in npcDialogueDic[sectionitem.NPCID])//��SID��λ
            {
                //�����������ɫ��DialogueSOװ�������ɫ��SectionSO
                foreach (KeyValuePair<int, DialogueSO> item in dic) //key��StartID
                {
                    /*Debug.Log(sectionitem.NPCID);
                    Debug.Log(item.Value);
                    Debug.Log(sectionitem.DialogueList.Count);*/
                    if (!sectionitem.DialogueList.Contains(item.Value))
                    {
                        sectionitem.DialogueList.Add(item.Value);
                    }

                    //�ѿ��������ɫ�ĶԻ�����������װ��SectionSO
                    if (ConditionList.ContainsKey(item.Key))
                    {
                        foreach (string conditionname in ConditionList[item.Key])
                        {
                            foreach (DialogueStatusSO statusitem in TalkSOManager.Instance.DialogueStatusListInstance)
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


        //Debug.Log(TalkSOManager.Instance.DialogueSectionListInstance.Count);
    }
}
