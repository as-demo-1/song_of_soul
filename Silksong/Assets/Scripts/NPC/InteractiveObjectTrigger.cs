using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveObjectTrigger : MonoBehaviour
{
    private GameObject m_dialog;
    private GameObject m_container;
    private Text m_text;

    public InteractiveSO InteractiveItem;

    void Start()
    {
        m_dialog = FindUI("Tip");
        m_container = FindUI("Tip/Container");
        m_text = FindUI("Tip/Container/Text").GetComponent<Text>();
        m_text.text = InteractiveItem.Content;
    }

#if UNITY_EDITOR
    void Update()
    {
        InteractiveItem.SetCoord(transform.position);
    }
#endif

    private GameObject FindUI(string path)
    {
        Transform trans = gameObject.transform.Find(path);
        if (trans != null)
            return gameObject.transform.Find(path).gameObject;

        throw new UnityException("not found gameobject " + path);
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

    private void CheckTriggerState(bool isEnter)
    {
        InteractManager.Instance.InteractObject = isEnter ? gameObject : null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CheckTriggerState(true);
            ToggleContent(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CheckTriggerState(false);
            ToggleContent(false);
            m_container.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
    }
}
