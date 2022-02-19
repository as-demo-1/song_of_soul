using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "DialogueStatus", menuName = "Dialogue/DialogueStatus")]
public class DialogueStatusSO : ScriptableObject
{
    [Tooltip("ConditionName")]
    [SerializeField] private string _conditionname = default;
    [Tooltip("是否达成")]
    [SerializeField] private bool _judge = default;

    public bool Judge => _judge;
    public string ConditionName => _conditionname;

    public static DialogueStatusSO CreateStatusSO(string conditionname, bool judge)
    {
        DialogueStatusSO asset = ScriptableObject.CreateInstance<DialogueStatusSO>();
        AssetDatabase.CreateAsset(asset, "Assets/Resources/SO_Status/" + conditionname + ".asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        asset._conditionname = conditionname;
        asset._judge = judge;
        return asset;
    }


    //public bool IsInit => _isInit;

}
