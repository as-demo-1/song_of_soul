using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class TalkManager : MonoBehaviour
{
    public Dictionary<int, string> TalkContent = new Dictionary<int, string>();//int�ǶԻ�id��string�ǶԻ�����
    public Dictionary<int, int> TalkNo = new Dictionary<int, int>(); //key�ǵ�ǰ�Ի�ID��value��NextID
    public Dictionary<int, string> TalkNPC = new Dictionary<int, string>();//int�ǶԻ�ID��string��˵����NPC����

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

        foreach (XmlNode xmlNode in xmlNodeList)//����<dialogues>�µ����нڵ�<dialogue>ѹ��List
        {
            XmlElement xmlElement = (XmlElement)xmlNode;//�����κ�һ��Ԫ�أ���ʵ����ÿһ��<dialogue>  
            TalkContent.Add(int.Parse(xmlElement.ChildNodes.Item(0).InnerText), xmlElement.ChildNodes.Item(1).InnerText);
            TalkNo.Add(int.Parse(xmlElement.ChildNodes.Item(0).InnerText), int.Parse(xmlElement.ChildNodes.Item(2).InnerText));
            TalkNPC.Add(int.Parse(xmlElement.ChildNodes.Item(0).InnerText), xmlElement.ChildNodes.Item(3).InnerText);
        }
    }

}
