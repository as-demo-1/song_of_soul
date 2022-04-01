using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Data;
using ExcelDataReader;

[CreateAssetMenu(fileName = "DialogueSection", menuName = "Dialogue/DialogueSection")]
public class DialogueSectionSO : ScriptableObject
{
    [Tooltip("Dialogue")]
    [SerializeField] public List<DialogueSO> DialogueList;
    [Tooltip("Dialogue status")]
    [SerializeField] public List<DialogueStatusSO> DialogueStatusList;
    [Tooltip("Dialogue NPC ID")]
    [SerializeField] private int _npcID = default;
    [Tooltip("Dialogue NPC Name")]
    [SerializeField] private string _npcname = default;

    //public List<DialogueSO> DialogueList => _dialogueList;
    //public List<DialogueStatusSO> DialogueStatusList => _dialogueStatusList;
    public string NPCName => _npcname;
    public int NPCID => _npcID;

    //[MenuItem("Dialogue/Create/CreateSectionSO", false, 2)]
    public static void CreateSectionSO()
    {
        TalkSOManager.Instance.DialogueSectionListInstance = new List<DialogueSectionSO>();
        foreach (int item in ExcelLoad.NPCID.Keys)
        {
            if (!Directory.Exists(Application.dataPath + "Assets/Resources/SO_Section/" + ExcelLoad.NPCName[ExcelLoad.NPCID[item][0]] + ".asset"))
            {
                DialogueSectionSO asset = ScriptableObject.CreateInstance<DialogueSectionSO>();
                AssetDatabase.CreateAsset(asset, "Assets/Resources/SO_Section/" + ExcelLoad.NPCName[ExcelLoad.NPCID[item][0]] + ".asset");
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
                asset._npcname = ExcelLoad.NPCName[ExcelLoad.NPCID[item][0]];
                asset._npcID = item;
                TalkSOManager.Instance.DialogueSectionListInstance.Add(asset);
            }
        }
    }
}