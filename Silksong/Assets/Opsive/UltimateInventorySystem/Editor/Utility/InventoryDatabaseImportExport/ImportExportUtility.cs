/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Utility.InventoryDatabaseImportExport
{
    using System;
    using System.Collections.Generic;
    using Opsive.UltimateInventorySystem.Storage;
    using UnityEngine;
    using System.Data;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The Import Export Module is a scriptable object used to import and export data from a database.
    /// </summary>
    public abstract class ImportExportModule : ScriptableObject
    {
        /// <summary>
        /// Export the inventory system database.
        /// </summary>
        /// <param name="database">The database to export.</param>
        public abstract void Export(InventorySystemDatabase database);
        
        /// <summary>
        /// Import the inventory system database.
        /// </summary>
        /// <param name="database">The database to import.</param>
        public abstract void Import(InventorySystemDatabase database);
    }

    public static class ImportExportUtility
    {
        
        public static string ObjectNameListToString(IReadOnlyList<ScriptableObject> scriptableObjects, string listDelimiter)
        {
            var result = "";
            if (scriptableObjects.Count == 0) {
                return result;
            }

            if (string.IsNullOrEmpty(listDelimiter)) {
                listDelimiter = " | ";
            }

            result += scriptableObjects[0].name;
            for (int i = 1; i < scriptableObjects.Count; i++) {
                result += listDelimiter+scriptableObjects[i].name;
            }

            return result;
        }
        
        public static string[] ObjectNameStringToList(string stringList, string listDelimiter)
        {
            return stringList.Split(new string[] { listDelimiter }, StringSplitOptions.None);
        }

        public static void WriteTablesToCSV(IReadOnlyList<DataTable> dataTables, string filePath) {
            StreamWriter sw = new StreamWriter(filePath, false);

            for (int i = 0; i < dataTables.Count; i++) {
                WriteTableToStreamWriter(dataTables[i], sw);
            }

            sw.Close();  
        }

        public static DataTable[] ReadCsvToDataTables(IReadOnlyList<string> headers, string filePath)
        {
            var dataTables = new DataTable[headers.Count];
            string all = System.IO.File.ReadAllText(filePath);
            
            // Split by row (not the same as splitting by line since there can be line breaks within the data.)
            string[] rows = Regex.Split(all, "\n(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            var currentDataTableIndex = -1;
            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                string row = rows[rowIndex];
                
                //Regex expression for splitting commas ignoring commas within data.
                string[] cells = Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                if (cells.Length < 2) {
                    continue; // Skip newlines
                }
                
                var foundHeader = false;
                for (int headerIndex = 0; headerIndex < headers.Count; headerIndex++) {
                    if (cells[0] != headers[headerIndex]) {
                        continue;
                    }

                    if (dataTables[headerIndex] != null) {
                        Debug.LogError($"ERROR ON IMPORT: The headers must be unique, the header '{cells[0]}' appeared at least twice.");
                        currentDataTableIndex = headerIndex;
                        foundHeader = true;
                        break;
                    }

                    dataTables[headerIndex] = new DataTable();
                    currentDataTableIndex = headerIndex;
                    foundHeader = true;
                    for (int k = 0; k < cells.Length; k++) {
                        dataTables[currentDataTableIndex].Columns.Add(cells[k].Trim());
                    }
                        
                    break;
                }
                
                //Ignore all lines before a matching header is found.
                if(currentDataTableIndex == -1){ continue; }
                    
                //a header was found wait next iteration to add content.
                if(foundHeader){ continue; }

                var dataTable = dataTables[currentDataTableIndex];
                for (int k = 0; k < cells.Length; k++) {
                    var c = cells[k];
                    if(c.Length > 1 == false){ continue; }

                    // Trim because the end of line isn't trimmed by default.
                    c = c.Trim();
                    // We add quotation marks when exporting to allows multi line support in data.
                    if (c[0] == '"' && c[c.Length - 1] == '"') {
                        c = c.Substring(1, c.Length-2);
                    }
                    cells[k] = c.Trim();
                }
                
                dataTable.Rows.Add(cells);
            }
            
            return dataTables;
        }
        
        public static void ToCSV(this DataTable dtDataTable, string strFilePath) {  
            StreamWriter sw = new StreamWriter(strFilePath, false);  
            //headers    
            WriteTableToStreamWriter(dtDataTable, sw);
            sw.Close();  
        }

        public static void WriteTableToStreamWriter(DataTable dataTable, StreamWriter sw)
        {
            for (int i = 0; i < dataTable.Columns.Count; i++) {
                sw.Write(dataTable.Columns[i]);
                if (i < dataTable.Columns.Count - 1) {
                    sw.Write(",");
                }
            }

            sw.Write(sw.NewLine);
            foreach (DataRow dr in dataTable.Rows) {
                for (int i = 0; i < dataTable.Columns.Count; i++) {
                    if (!Convert.IsDBNull(dr[i])) {
                        string value = dr[i].ToString();
                        value = String.Format("\"{0}\"", value);
                        sw.Write(value);
                    }

                    if (i < dataTable.Columns.Count - 1) {
                        sw.Write(",");
                    }
                }

                sw.Write(sw.NewLine);
            }
        }
    }

   
}
