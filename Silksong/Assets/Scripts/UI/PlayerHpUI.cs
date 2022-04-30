using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpUI : MonoBehaviour
{
    public HpDamable representedDamable;
    public GameObject healthIconPrefab;

    protected Animator[] m_HealthIconAnimators;

    protected readonly int m_HashActivePara = Animator.StringToHash("Active");
   // protected readonly int m_HashInactiveState = Animator.StringToHash("Inactive");
    protected const float heartIcon_XWidth = 60f;

    public void setRepresentedDamable(HpDamable hpDamable)//设置血量上限
    {
        if (representedDamable == null)
        {
            representedDamable = hpDamable;
            representedDamable.onHpChange.AddListener(ChangeHitPointUI);
        }
        else
        {
            deleteAllHpIcon();
        }



        m_HealthIconAnimators = new Animator[representedDamable.MaxHp];

        for (int i = 0; i < representedDamable.MaxHp; i++)
        {
            GameObject healthIcon = Instantiate(healthIconPrefab);
            healthIcon.transform.SetParent(transform,false);

            RectTransform healthIconRect = healthIcon.transform as RectTransform;
            Vector3 t = healthIconRect.localPosition;
            healthIconRect.localPosition = new Vector3(t.x + heartIcon_XWidth * i, t.y, t.z);

            m_HealthIconAnimators[i] = healthIcon.GetComponent<Animator>();
            
        }
    }

    public void ChangeHitPointUI(HpDamable damageable)//血量变动时调用
    {
        if (m_HealthIconAnimators == null)
            return;

        for (int i = 0; i < m_HealthIconAnimators.Length; i++)
        {
            m_HealthIconAnimators[i].SetBool(m_HashActivePara, damageable.CurrentHp >= i + 1);
        }
    }

    private void deleteAllHpIcon()
    {
        for(int i=0;i<transform.childCount;i++)
        {
            Destroy( transform.GetChild(i));
        }
    }

    public static void setSceneNameText(string name) //show scene name for test,will be delete
    {
        Text text = GameObject.Find("SceneName").GetComponent<Text>();
        text.text = name;
    }
}
