using System.Collections;
using System.Collections.Generic;
using Excel;
using System.Data;
using System.IO;
using UnityEngine;
using UnityEditor;

public class ExcelBuild : Editor
{
        [MenuItem("CustomEditor/CreateItemAsset")]
         public static void CreateWeaponUpdate()
        {
            WeaponUpdate manager = ScriptableObject.CreateInstance<WeaponUpdate>();
            //赋值
            manager.weaponUpgradeInfoList = ExcelReader.CreateWeaponUpgradeInfoWithExcel(ExcelConfig.excelsFolderPath + "weapon.xlsx");

            //确保文件夹存在
            if (!Directory.Exists(ExcelConfig.assetPath))
            {
                Directory.CreateDirectory(ExcelConfig.assetPath);
            }

            //asset文件的路径 要以"Assets/..."开始，否则CreateAsset会报错
            string assetPath = string.Format("{0}{1}.asset", ExcelConfig.assetPath, "Item");
            //生成一个Asset文件
            AssetDatabase.CreateAsset(manager, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
       }

}
