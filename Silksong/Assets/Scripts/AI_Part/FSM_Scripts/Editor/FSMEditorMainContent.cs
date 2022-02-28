using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FSMEditorMainContent :EditorWindow
{
    [MenuItem("Window/MonsterEditor")]
    static void Init()
    {
        var myWindow = GetWindow<FSMEditorMainContent>("MonsterEditor");
        myWindow.Show();
    }
}
