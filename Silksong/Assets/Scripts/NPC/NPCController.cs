using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NPCController : MonoBehaviour
{
    private GameObject m_dialog;
    private GameObject m_container;
    private Text m_text;

    public InteractiveSO InteractiveItem;

    // Start is called before the first frame update
    void Start()
    {
        m_dialog = FindUI("Tip");
        m_container = FindUI("Tip/Container");
        m_text = FindUI("Tip/Container/Text").GetComponent<Text>();
    }
#if UNITY_EDITOR
    void Update()
    {
        InteractiveItem.SetCoord(transform.position);
    }
#endif

    private GameObject FindUI(string path)
    {
        return gameObject.transform.Find(path).gameObject;
    }

    private void CheckTriggerState(EInteractiveItemType itemType, bool isEnter)
    {
        InteractManager.Instance.ItemType = itemType;
        InteractManager.Instance.IsOnTrigger = isEnter;
        if (itemType == EInteractiveItemType.NONE)
        {
            InteractManager.Instance.FuncInteractItemType = (InteractiveItem as FuncInteractiveSO).FuncInteractItemType;
        }
        else if (itemType == EInteractiveItemType.DIALOG)
        {
            InteractManager.Instance.InteractiveItemPos = GetCoord((InteractiveItem as NPCSO).TalkCoord);
            InteractManager.Instance.InteractiveItemID = InteractiveItem.ID;
        }
    }

    private Vector3 GetCoord(Vector3 coord)
    {
        return transform.position + coord;
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
            m_text.text = InteractiveItem.Content;
            CheckTriggerState(InteractiveItem.ItemType, true);
            ToggleContent(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            CheckTriggerState(InteractiveItem.ItemType, false);
            ToggleContent(false);
            m_container.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
    }
}
