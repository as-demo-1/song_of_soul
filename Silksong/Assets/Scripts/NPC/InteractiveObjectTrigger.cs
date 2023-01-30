using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveObjectTrigger : MonoBehaviour
{
    private GameObject m_dialog;
    private GameObject m_container;

    public InteractiveBaseSO InteractiveItem;

    void Start()
    {
        m_dialog = UIComponentManager.Instance.FindUI(gameObject, InteractConstant.UITip);
        m_container = UIComponentManager.Instance.FindUI(gameObject, InteractConstant.UITipContainer);
    }

#if UNITY_EDITOR
    void Update()
    {
        InteractiveItem.SetPosition(gameObject, transform.position);
    }
#endif

    private void ToggleContent(bool isEnter)
    {
        // todo:
        // --最好有动画
        m_dialog.SetActive(isEnter);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            InteractManager.Instance.CollidingObject = gameObject;
            ToggleContent(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            InteractManager.Instance.CollidingObject = null;
            ToggleContent(false);
            m_container.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
    }
}
