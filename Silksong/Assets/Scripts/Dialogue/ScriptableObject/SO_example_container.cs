using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class SO_example_container : ScriptableObject
{
    public List<SO_example> SO_list;

    [MenuItem("SO_objects/Create/CreateSO_example_Container", false, 1)]
    public static void CreateSO_example()
    {
        SO_example_container asset = ScriptableObject.CreateInstance<SO_example_container>();
        AssetDatabase.CreateAsset(asset, "Assets/Resources/SO_Container/testSOContainer.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
