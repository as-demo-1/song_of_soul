using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NPCController : MonoBehaviour
{
    public int ItemID;
    private GameObject m_dialog;
    private GameObject m_container;
    private Text m_text;
    private Vector3 m_dialogPos;
    private InteractiveItemConfig m_info;

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
                DialogInteract.Instance.TalkComponent = GetComponent<TalkAct>();
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

    private void ToggleContent(bool isEnter)
    {
        // todo:
        // --最好有动画
        m_dialog.SetActive(isEnter);
    }

    private void ToggleContent(string content, bool isEnter)
    {
        // 如果需要切换文字的话

        m_dialog.SetActive(isEnter);
        m_text.text = content;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            // todo:loading功能实现后之后awake
            m_info = InteractiveItemConfigManager.Instance.ItemConfig[ItemID];
            m_text.text = m_info.Content;
            CheckTriggerState(m_info.ItemType, true);
            ToggleContent(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            CheckTriggerState(m_info.ItemType, false);
            ToggleContent(false);
            m_container.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
    }
}
