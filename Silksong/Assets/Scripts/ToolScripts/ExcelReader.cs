using System.Collections;
using System.Collections.Generic;
using Excel;
using System.Data;
using System.IO;
using UnityEngine;
    public class ExcelReader 
    {
        public static WeaponUpgradeInfo[] CreateWeaponUpgradeInfoWithExcel(string filePath)
        {
            int columnNum = 0, rowNum = 0;
            DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);

            WeaponUpgradeInfo[] array = new WeaponUpgradeInfo[rowNum - 2];
            for (int i = 2; i < rowNum; i++)
            {
                WeaponUpgradeInfo item = new WeaponUpgradeInfo();
                item.level = int.Parse(collect[i][0].ToString());
                item.attack = int.Parse(collect[i][1].ToString());
                for (int j = 2; j < columnNum; j++)
                {
                    RequiredMaterial requiredMaterial;
                    requiredMaterial.id = collect[i][j].ToString();
                    requiredMaterial.amonut = int.Parse(collect[i][++j].ToString());
                    item.requiredMaterial.Add(requiredMaterial);
                }
                array[i - 2] = item;
            }
            return array;
        }

        static DataRowCollection ReadExcel(string filePath, ref int columnNum, ref int rowNum)
        {
            FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

            DataSet result = excelReader.AsDataSet();
            columnNum = result.Tables[0].Columns.Count;
            rowNum = result.Tables[0].Rows.Count;
            return result.Tables[0].Rows;
        }
    }
