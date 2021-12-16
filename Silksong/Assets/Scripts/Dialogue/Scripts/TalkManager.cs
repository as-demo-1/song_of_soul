using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class TalkManager : MonoBehaviour
{
    public Dictionary<int, string> TalkContent = new Dictionary<int, string>();//int是对话id，string是对话内容
    public Dictionary<int, int> TalkNo = new Dictionary<int, int>(); //key是当前对话ID，value是NextID
    public Dictionary<int, string> TalkNPC = new Dictionary<int, string>();//int是对话ID，string是说话的NPC名字

    XmlDocument xmlDocument = new XmlDocument();

    public static TalkManager Instance;


    public void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        string data = Resources.Load("XML/DialogueContent").ToString();
        xmlDocument.LoadXml(data);
        XmlNodeList xmlNodeList = xmlDocument.SelectSingleNode("dialogues").ChildNodes;

        foreach (XmlNode xmlNode in xmlNodeList)//遍历<dialogues>下的所有节点<dialogue>压入List
        {
            XmlElement xmlElement = (XmlElement)xmlNode;//对于任何一个元素，其实就是每一个<dialogue>  
            TalkContent.Add(int.Parse(xmlElement.ChildNodes.Item(0).InnerText), xmlElement.ChildNodes.Item(1).InnerText);
            TalkNo.Add(int.Parse(xmlElement.ChildNodes.Item(0).InnerText), int.Parse(xmlElement.ChildNodes.Item(2).InnerText));
            TalkNPC.Add(int.Parse(xmlElement.ChildNodes.Item(0).InnerText), xmlElement.ChildNodes.Item(3).InnerText);
        }
    }

}
