using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/Dialogue")]
public class DialogueSO : ScriptableObject
{
    [Tooltip("The start id of the dialogue")]
    [SerializeField] private int _startid = default;
    [Tooltip("The end id of the dialogue")]
    [SerializeField] private int _endid = default;
    [Tooltip("The content of the dialogue")]
    [SerializeField] private List<string> _content = default;
    [Tooltip("The NPCID")]
    [SerializeField] private int _npcid = default;
    [Tooltip("Type of Content")]
    [SerializeField] string _type = default;

    [Tooltip("控制这段对话的条件")]
    [SerializeField] private List<string> _StatusList = default;

    
    public int StartID => _startid;
    public int EndID => _endid;
    public int NPCID => _npcid;
    public string Type => _type;
    public List<string> Content => _content;
    public List<string> StatusList => _StatusList;

    public static DialogueSO CreateSO(string startID,string endID,int npcid,string type)
    {
        DialogueSO asset = ScriptableObject.CreateInstance<DialogueSO>();
        AssetDatabase.CreateAsset(asset, "Assets/Resources/SO_objects/" + startID + "-" + endID + ".asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        asset._startid = int.Parse(startID);
        asset._endid = int.Parse(endID);
        asset._npcid = npcid;
        asset._type = type;
        return asset;
    }
}
