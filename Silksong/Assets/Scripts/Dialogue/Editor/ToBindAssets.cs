using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ToBindAssets
{
	public static SO_example_container temp_container;
	[MenuItem("SO_objects/Operation/ToBindContainer", false, 1)]
	public static void ToBindTheContainer()
	{
		string SoContainerPath = "Assets/Resources/SO_Container/testSOContainer.asset";
		//Debug.Log(Application.dataPath);
        if (File.Exists(SoContainerPath))
        {
			Debug.Log(File.Exists(SoContainerPath));
			SO_example_container temp = Resources.Load<SO_example_container>("SO_Container/" + "testSOContainer");
			Debug.Log(temp.name);
			if (temp != null)
			{
				temp_container = SO_example_container.CreateInstance<SO_example_container>();
				temp.SO_list = new List<SO_example>(0);
				temp_container.SO_list = temp.SO_list ;
				Debug.Log("绑定对应路径的Container");
			}
			else if(temp == null) {
				Debug.Log("绑定对应路径的Container失败");
			}

        }
        else{
			Debug.Log("路径下Container不存在");
		}
	}
	
	[MenuItem("SO_objects/Operation/ToBindAssetForContainer", false, 2)]
    public static void  ToFIndAndBindAssetForContainer() {
		//读取目录路径
		string path = "Assets/Resources/SO_objects/";
		//目录存在则开始读取所有对应类型文件
		if (Directory.Exists(path))
		{
			Debug.Log("目录正确，开始查找");
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
			for (int i = 0; i < fileInfos.Length; i++)
			{
				if (fileInfos[i].Name.EndsWith(".asset"))
				{
					if (fileInfos[i].Name.Contains("SO_"))//增加其他限制条件避免查找出错
					{
						Debug.Log("找到对应SO物体:" + fileInfos[i].Name);
						if (temp_container != null)
						{
							SO_example temp = SO_example.CreateInstance<SO_example>();
							string FileName = fileInfos[i].Name.Substring(0, fileInfos[i].Name.Length - 6);
							Debug.Log("加载SO物体:"+fileInfos[i].Name.Substring(0, fileInfos[i].Name.Length - 6));
							temp =Resources.Load<SO_example>("SO_objects/" +FileName);
							if (temp != null)
							{
								//temp_container.SO_list = new List<SO_example>(0);
								temp_container.SO_list.Add(temp);
							}
							else {
								Debug.Log("So_example文件读取出错，请检查");
							}
						}
					}
				}
				
			}

		}
		else {
			Debug.Log("当前规定目录不存在");
		}
		
    }

}
    

