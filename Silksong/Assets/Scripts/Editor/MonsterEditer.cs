using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Data;
using System.IO;
using Excel;

public class MonsterEditer : EditorWindow
{
    private DefaultAsset _sourceExcelText;
    private string _tip_text;

    string _path = "/Resources/ScriptableObjects/monsters";

    [MenuItem("Importer/Monsters", false)]

    public static void Init()
    {
        var window = GetWindow<MonsterEditer>();
        window.titleContent = new GUIContent("怪物表导入工具");
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
        _sourceExcelText = EditorGUILayout.ObjectField("monsters excel file", _sourceExcelText, typeof(DefaultAsset), false) as DefaultAsset;

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
        Debug.Log(row);

        string dataPath = Application.dataPath;
        string folderPath = dataPath + _path;
        //string saveDirectory = new FileInfo(AssetDatabase.GetAssetPath(sourceObject)).DirectoryName;

        //folderPath = EditorUtility.SaveFolderPanel("Save Cash Questions! ScriptObject Files", saveDirectory, sourceObject.name);

        if (folderPath.IndexOf(dataPath, System.StringComparison.InvariantCultureIgnoreCase) == -1)
        {
            Debug.LogError("你的存储路径路径有误， 请选择 \"" + dataPath + "\" 中的路径");
            return;
        }
        _tip_text = "准备生成so文件";
        string relativeFolderPath = folderPath.Substring(dataPath.Length - 6);

        //获得每行的名字，并添加到数组里
        for (int i = 3; i < row; ++i)
        {
            if (resultds.Tables[0].Rows[i][0].ToString() == "")
            {
                continue;
            }
            _tip_text = "正在生成第" + i + "个so文件";
            string id = resultds.Tables[0].Rows[i][0].ToString();
            string nameSid = resultds.Tables[0].Rows[i][1].ToString();
            string descSid = resultds.Tables[0].Rows[i][2].ToString();
            string health = resultds.Tables[0].Rows[i][3].ToString();
            string damage = resultds.Tables[0].Rows[i][4].ToString();
            string speed = resultds.Tables[0].Rows[i][5].ToString();
            string drop1 = resultds.Tables[0].Rows[i][6].ToString();
            string drop2 = resultds.Tables[0].Rows[i][7].ToString();
            string drop3 = resultds.Tables[0].Rows[i][8].ToString();

            string fileName = id;
            string tex_Path_NoExt = relativeFolderPath + "/" + fileName;

            MonsterScriptableObject monsterAsset = AssetDatabase.LoadAssetAtPath<MonsterScriptableObject>(tex_Path_NoExt + ".asset");

            // new
            if (monsterAsset == null)
            {
                monsterAsset = ScriptableObject.CreateInstance<MonsterScriptableObject>();
                monsterAsset.Crt(id, nameSid, descSid, health, damage, speed, drop1, drop2, drop3);
                AssetDatabase.CreateAsset(monsterAsset, tex_Path_NoExt + ".asset");
            }
            // update
            else
            {
                monsterAsset.Upd(nameSid, descSid, health, damage, speed, drop1, drop2, drop3);
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

        for (int i = 3; i < row; ++i)
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

