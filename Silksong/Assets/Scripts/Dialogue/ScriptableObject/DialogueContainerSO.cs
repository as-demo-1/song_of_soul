using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CreateAssetMenu(fileName = "DialogueContainer", menuName = "Dialogue/DialogueContainer")]
public class DialogueContainerSO : ScriptableObject
{
    public List<DialogueSectionSO> DialogueSectionList;
    public List<DialogueStatusSO> DialogueStatusList;
    public List<DialogueSO> DialogueList;


    public static void CreateSO_example()
    {
        DialogueContainerSO asset = ScriptableObject.CreateInstance<DialogueContainerSO>();
        AssetDatabase.CreateAsset(asset, "Assets/Resources/DialogueContainer.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
