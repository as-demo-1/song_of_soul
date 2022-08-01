using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Data;
using System.IO;
using Excel;

public class ItemTableEditor : EditorWindow
{
    private DefaultAsset _sourceExcelText;
    private string _tip_text;

    string _path = "/Resources/ScriptableObjects/items";
    string _path_so = "/Resources/ScriptableObjects/itemso";

    [MenuItem("Importer/Items", false)]

    public static void Init()
    {
        var window = GetWindow<ItemTableEditor>();
        window.titleContent = new GUIContent("道具表导入工具");
        window.Focus();
    }

    public void OnEnable()
    {
        _tip_text = "准备导入";
    }

    public void OnDisable()
    {
        _tip_text = "";
    }

    public void OnGUI()
    {
        GUILayout.BeginHorizontal();
        DrawControls();
        GUILayout.EndHorizontal();
    }

    void DrawControls()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Label("Import Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUI.BeginChangeCheck();
        _sourceExcelText = EditorGUILayout.ObjectField("items excel file", _sourceExcelText, typeof(DefaultAsset), false) as DefaultAsset;

        if (EditorGUI.EndChangeCheck())
        {
        }

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("生成so文件"))
        {
            SaveNewScriptableObject(_sourceExcelText);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("清除多余的so文件"))
        {
            DelScriptableObject(_sourceExcelText);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        GUILayout.Label(_tip_text);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
    }

    private DataSet ReadExcel(string path)
    {
        _tip_text = "正在读取excel...";
        FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

        DataSet result = excelReader.AsDataSet();
        excelReader.Close();
        _tip_text = "读取excel完成";
        return result;
    }

    /// <summary>
    /// 生成so文件
    /// </summary>
    /// <param name="sourceObject">excel filepath</param>
    void SaveNewScriptableObject(DefaultAsset sourceObject)
    {
        DataSet resultds = ReadExcel(AssetDatabase.GetAssetPath(sourceObject));
        int column = resultds.Tables[0].Columns.Count;
        int row = resultds.Tables[0].Rows.Count;

        string itemTypePath = "Assets/Resources/ScriptableObjects/ItemType.asset";
        ItemTypeSO itemTypeAsset = AssetDatabase.LoadAssetAtPath<ItemTypeSO>(itemTypePath);

        string dataPath = Application.dataPath;
        string folderPath = dataPath + _path;
        string folderPath2 = dataPath + _path_so;
        //string saveDirectory = new FileInfo(AssetDatabase.GetAssetPath(sourceObject)).DirectoryName;

        //folderPath = EditorUtility.SaveFolderPanel("Save Cash Questions! ScriptObject Files", saveDirectory, sourceObject.name);

        if (folderPath.IndexOf(dataPath, System.StringComparison.InvariantCultureIgnoreCase) == -1)
        {
            Debug.LogError("你的存储路径路径有误， 请选择 \"" + dataPath + "\" 中的路径");
            return;
        }
        _tip_text = "准备生成so文件";
        string relativeFolderPath = folderPath.Substring(dataPath.Length - 6);
        string relativeFolderPath2 = folderPath2.Substring(dataPath.Length - 6);

        //获得每行的名字，并添加到数组里
        for (int i = 4; i < row; ++i)
        {
            _tip_text = "正在生成第" + i + "个so文件";
            string id = resultds.Tables[0].Rows[i][0].ToString();
            string nameSid = resultds.Tables[0].Rows[i][1].ToString();
            string descSid = resultds.Tables[0].Rows[i][2].ToString();
            string buffId = resultds.Tables[0].Rows[i][3].ToString();
            string buffVal = resultds.Tables[0].Rows[i][4].ToString();

            string fileName = id;
            string tex_Path_NoExt = relativeFolderPath + "/" + fileName;
            string tex_Path_NoExt2 = relativeFolderPath2 + "/" + fileName;

            ItemTableSO itemAsset = AssetDatabase.LoadAssetAtPath<ItemTableSO>(tex_Path_NoExt + ".asset");
            ItemSO itemAsset2 = AssetDatabase.LoadAssetAtPath<ItemSO>(tex_Path_NoExt2 + ".asset");

            // new
            if (itemAsset2 == null)
            {
                itemAsset2 = ScriptableObject.CreateInstance<ItemSO>();
                AssetDatabase.CreateAsset(itemAsset2, tex_Path_NoExt2 + ".asset");
            }

            // new
            if (itemAsset == null)
            {
                itemAsset = ScriptableObject.CreateInstance<ItemTableSO>();
                itemAsset.Crt(id, nameSid, descSid, buffId, buffVal, itemAsset2, itemTypeAsset);
                AssetDatabase.CreateAsset(itemAsset, tex_Path_NoExt + ".asset");
            }
            // update
            else
            {
                itemAsset.Upd(nameSid, descSid, buffId, buffVal);
            }
        }
        _tip_text = "已在" + relativeFolderPath + "下生成so文件";
    }

    /// <summary>
    /// 清理文件夹下多余的文件
    /// </summary>
    /// <param name="sourceObject">excel filepath</param>
    private void DelScriptableObject(DefaultAsset sourceObject)
    {
        DataSet resultds = ReadExcel(AssetDatabase.GetAssetPath(sourceObject));
        int column = resultds.Tables[0].Columns.Count;
        int row = resultds.Tables[0].Rows.Count;

        string dataPath = Application.dataPath;
        string folderPath = dataPath + _path;
        //string saveDirectory = new FileInfo(AssetDatabase.GetAssetPath(sourceObject)).DirectoryName;

        //folderPath = EditorUtility.SaveFolderPanel("Save Cash Questions! ScriptObject Files", saveDirectory, sourceObject.name);

        if (folderPath.IndexOf(dataPath, System.StringComparison.InvariantCultureIgnoreCase) == -1)
        {
            Debug.LogError("你的存储路径路径有误， 请选择 \"" + dataPath + "\" 中的路径");
            return;
        }
        _tip_text = "准备清除so文件";
        string relativeFolderPath = folderPath.Substring(dataPath.Length - 6);

        HashSet<string> ids = new HashSet<string>();

        if (Directory.Exists(relativeFolderPath))
        {
            DirectoryInfo direction = new DirectoryInfo(relativeFolderPath);
            FileInfo[] files = direction.GetFiles("*");
            for (int i = 0; i < files.Length; ++i)
            {
                if (files[i].Name.EndsWith(".meta"))
                    continue;

                ids.Add(files[i].Name);
            }
        }

        for (int i = 4; i < row; ++i)
        {
            string id = resultds.Tables[0].Rows[i][0].ToString();
            ids.Remove(id + ".asset");
        }

        foreach (string filename in ids)
        {
            File.Delete(relativeFolderPath + "/" + filename);
            File.Delete(relativeFolderPath + "/" + filename + ".meta");
        }

        _tip_text = "清除完毕";
    }
}

