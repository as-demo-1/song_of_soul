using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Data;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/Dialogue")]
public class DialogueSO : ScriptableObject
{
    [Tooltip("The start id of the dialogue")]
    [SerializeField] private int _startid = default;
    [Tooltip("The end id of the dialogue")]
    [SerializeField] private int _endid = default;
    [Tooltip("The NPCID")]
    [SerializeField] private int _npcid = default;
    [Tooltip("Type of Content")]
    [SerializeField] string _type = default;

    
    public int StartID => _startid;
    public int EndID => _endid;
    public int NPCID => _npcid;
    public string Type => _type;
    public List<string> Content;
    public List<string> StatusList;

    //[MenuItem("Dialogue/Create/CreateSO", false, 2)]
    public static void MultyCreateSO()
    {
        TalkSOManager.Instance.DialogueListInstance = new List<DialogueSO>();
        foreach (KeyValuePair<int, int> item in ExcelLoad.SIDList)
        {
            if (!Directory.Exists(Application.dataPath + "/Resources/SO_Objects/" + int.Parse(item.Key.ToString()) + "-" + int.Parse(item.Value.ToString()) + ".asset"))
            {
                DialogueSO asset = ScriptableObject.CreateInstance<DialogueSO>();
                AssetDatabase.CreateAsset(asset, "Assets/Resources/SO_objects/" + int.Parse(item.Key.ToString()) + "-" + int.Parse(item.Value.ToString()) + ".asset");
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
                asset._startid = int.Parse(item.Key.ToString());
                asset._endid = int.Parse(item.Value.ToString());
                asset._npcid = ExcelLoad.SidGetNpcID[item.Key];
                asset._type = ExcelLoad.type[item.Key];
                //TalkSOManager.DialogueList.Add(asset);
                TalkSOManager.Instance.DialogueListInstance.Add(asset);
                //Debug.Log(asset._npcid);
            }
        }
    }
}
