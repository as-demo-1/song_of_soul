using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractController : MonoBehaviour
{
    private GameObject m_dialog;
    private GameObject m_container;

    public InactItemSO InactItem;
    public Queue<MoveEventAction> MoveEventActions = new Queue<MoveEventAction>();

    void Start()
    {
        m_dialog = UIComponentManager.Instance.FindUI(gameObject, InteractConstant.UITip);
        m_container = UIComponentManager.Instance.FindUI(gameObject, InteractConstant.UITipContainer);
        InactItem.AddGo(gameObject);
    }

    void Update()
    {
        if (MoveEventActions.Count != 0)
        {
            MoveEventAction mea = MoveEventActions.Dequeue();
            mea.time -= 1;
            if (mea.time > -0.00001f)
            {
                transform.position = Vector3.Lerp(mea.pos2, mea.pos1, mea.time / mea.step);
                MoveEventActions.Enqueue(mea);
            }
            else
            {
                mea.action();
            }
        }
    }

    public void Next()
    {
        InactItem.Next();
    }

    private void ToggleContent(bool isEnter)
    {
        // todo:
        // --最好有动画
        m_dialog.SetActive(isEnter);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.tag);
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
