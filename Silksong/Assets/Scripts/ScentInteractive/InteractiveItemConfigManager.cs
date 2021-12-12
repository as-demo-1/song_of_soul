using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using UnityEditor;

public class InteractiveItemConfigManager
{
    XmlDocument xmlDocument = new XmlDocument();

    private Dictionary<int, InteractiveItemConfig> _itemConfig = new Dictionary<int, InteractiveItemConfig>();

    public void Load()
    {
        string data = Resources.Load("XML/InteractItem").ToString();
        xmlDocument.LoadXml(data);
        XmlNodeList xmlNodeList = xmlDocument.SelectSingleNode("items").ChildNodes;

        foreach (XmlNode xmlNode in xmlNodeList)
        {
            XmlElement xmlElement = (XmlElement)xmlNode;
            
            int id = int.Parse(xmlElement.SelectSingleNode("ID").InnerText);
            string name = xmlElement.SelectSingleNode("name").InnerText;
            InteractiveItemType type = (InteractiveItemType)int.Parse(xmlElement.SelectSingleNode("type").InnerText);
            string content = xmlElement.SelectSingleNode("content").InnerText;
            _itemConfig[id] = new InteractiveItemConfig(id, name, type, content);
        }
    }

    private static InteractiveItemConfigManager s_instance;
    public static InteractiveItemConfigManager Instance
    {
        get
        {
            if (s_instance == null)
                s_instance = new InteractiveItemConfigManager();

            return s_instance;
        }
    }

    public Dictionary<int, InteractiveItemConfig> ItemConfig
    {
        get { return _itemConfig; }
    }
}

public enum InteractiveItemType
{
    NONE = -1,
    DIALOG,
    JUDGE,
    FULLWINDOW
}

public class InteractiveItemConfig
{
    public int ID { get; private set; }
    public string Name { get; private set; }
    public InteractiveItemType ItemType { get; private set; }
    public string Content { get; private set; }

    public InteractiveItemConfig(int id, string name, InteractiveItemType itemType, string content)
    {
        ID = id;
        Name = name;
        ItemType = itemType;
        Content = content;
    }
}
