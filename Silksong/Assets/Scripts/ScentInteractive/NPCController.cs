using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NPCController : MonoBehaviour
{
    //if it is an item, it can use ItemSO GUID for ItemID
    public int ItemID;
    public ItemSO m_Item;
    private GameObject m_dialog;
    private GameObject m_container;
    private Text m_text;
    private Vector3 m_dialogPos;

    public GameObject Tip;

    public float Distance = 5.0f;

    // Start is called before the first frame update
    void Awake()
    {
        Tip = AssetDatabase.LoadAssetAtPath("Assets/Scripts/ScentInteractive/Prefabs/Tip.prefab", typeof(GameObject)) as GameObject;

        Instantiate(Tip, transform);

        m_dialog = gameObject.transform.Find("Tip(Clone)").gameObject;
        if (m_dialog == null)
        {
            Debug.LogError("Tip(Clone) not found");
        }
        m_container = gameObject.transform.Find("Tip(Clone)/Container").gameObject;
        if (m_container == null)
        {
            Debug.LogError("m_back not found");
        }
        GameObject txtWarp = gameObject.transform.Find("Tip(Clone)/Container/Text").gameObject;
        if (txtWarp == null)
        {
            Debug.LogError("txtWarp not found");
        }
        else
        {
            m_text = txtWarp.GetComponent<Text>();
            if (m_text == null)
            {
                Debug.LogError("m_text not found");
            }
        }

        ItemID = int.Parse(gameObject.name);
        m_dialogPos = new Vector3(transform.position.x + Distance, transform.position.y, transform.position.z);
    }

    private void CheckTriggerState(InteractiveItemType itemType, bool isEnter)
    {
        switch (itemType)
        {
            // 对话框事件
            case InteractiveItemType.DIALOG:
                DialogInteract.Instance.IsOnTrigger = isEnter;
                DialogInteract.Instance.TriggerItemType = itemType;
                DialogInteract.Instance.DialogPos = m_dialogPos;
                break;
            // 普通事件
            case InteractiveItemType.NONE:
            case InteractiveItemType.FULLWINDOW:
            case InteractiveItemType.JUDGE:
                NormaInteract.Instance.IsOnTrigger = isEnter;
                NormaInteract.Instance.TriggerItemType = itemType;
                break;
            default:
                break;
        }
    }

    private void ToggleContent(string content, bool isEnter)
    {
        // todo:
        // --最好有动画
        m_dialog.SetActive(isEnter);
        m_text.text = content;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            int itemID = ItemID;

           // InteractiveItemConfig info
           //    = InteractiveItemConfigManager.Instance.config[itemID];
           // InteractiveItemType itemType = info.getItemType();
           // string content = info.getContent();

            CheckTriggerState(m_Item.m_itemType, true);
            ToggleContent(m_Item.Description, true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            int itemID = ItemID;

            //InteractiveItemConfig info
            //    = InteractiveItemConfigManager.Instance.config[itemID];
            //InteractiveItemType itemType = info.getItemType();
            CheckTriggerState(m_Item.m_itemType, false);
            ToggleContent(m_Item.Description, false);
            m_container.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
    }
}
