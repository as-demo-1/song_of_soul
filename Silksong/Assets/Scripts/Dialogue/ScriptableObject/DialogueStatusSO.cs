using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Data;

[CreateAssetMenu(fileName = "DialogueStatus", menuName = "Dialogue/DialogueStatus")]
public class DialogueStatusSO : ScriptableObject
{
    [Tooltip("ConditionName")]
    [SerializeField] private string _conditionname = default;
    [Tooltip("是否达成")]
    [SerializeField] private bool _judge = default;

    public bool Judge => _judge;
    public string ConditionName => _conditionname;

    //[MenuItem("Dialogue/Create/CreateStatusSO", false, 2)]
    public static void CreateStatusSO()
    {
        TalkSOManager.Instance.DialogueStatusListInstance = new List<DialogueStatusSO>();
        foreach (KeyValuePair<int, int> item in ExcelLoad.SIDList)
        {
            if (ExcelLoad.ConditionList.ContainsKey(item.Key))
            {
                foreach (string conditionName in ExcelLoad.ConditionList[item.Key])
                {
                    if (!Directory.Exists(Application.dataPath + "Assets/Resources/SO_Status/" + conditionName + ".asset"))
                    {
                        DialogueStatusSO asset = ScriptableObject.CreateInstance<DialogueStatusSO>();
                        AssetDatabase.CreateAsset(asset, "Assets/Resources/SO_Status/" + conditionName + ".asset");
                        AssetDatabase.SaveAssets();
                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = asset;
                        asset._conditionname = conditionName;
                        asset._judge = false;
                        TalkSOManager.Instance.DialogueStatusListInstance.Add(asset);
                    }
                }
            }
        }
    }


    //public bool IsInit => _isInit;

}
