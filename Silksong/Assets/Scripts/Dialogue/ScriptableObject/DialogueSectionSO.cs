using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

    public static DialogueSectionSO CreateSectionSO(string npcname,int npcid)
    {
        DialogueSectionSO asset = ScriptableObject.CreateInstance<DialogueSectionSO>();
        AssetDatabase.CreateAsset(asset, "Assets/Resources/SO_Section/" + npcname + ".asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        asset._npcname = npcname;
        asset._npcID = npcid;
        return asset;
    }
}
