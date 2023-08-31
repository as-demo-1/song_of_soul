using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class SwitchSoulSkill : SerializedMonoBehaviour
{
    public Dictionary<SkillName, GameObject> skillDic = new Dictionary<SkillName, GameObject>();

    [SerializeField]
    private SkillName selectSkill;

    public Transform panel;

    public float xGap;
    //private PanelView _panelView;
    // Start is called before the first frame update
    void Start()
    {
        //_panelView = GetComponent<PanelView>();
        //_panelView.OnPanelChange.AddListener(ChangeSoulSkill);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        foreach (var skillItem in PlayerController.Instance.SoulSkillController.soulSkillList)
        {
            if (skillItem.hasLearned && skillDic.ContainsKey(skillItem.SkillName))
            {
                skillDic[skillItem.SkillName].SetActive(true);
            }
        }

        selectSkill = PlayerController.Instance.SoulSkillController.equpedSoulSkill.skillName;
        DOVirtual.DelayedCall(0.2f,()=>ChangeSelect(0));

    }
    
    public void OnClickPre()
    {
        
        ChangeSelect(-1);
        Debug.Log(selectSkill);
    }

    public void OnClickNext()
    {
        ChangeSelect(1);
        Debug.Log(selectSkill);
    }

    private void ChangeSelect(int option)
    {
        for (int i = 1; i < 4; i++)
        {
            if (skillDic.ContainsKey(selectSkill + option * i) &&
                skillDic[selectSkill + option*i].activeInHierarchy)
            {
                selectSkill += option*i;
                PlayerController.Instance.SoulSkillController.ChangeEquip(selectSkill);
                panel.DOLocalMove(
                    new Vector3(-skillDic[selectSkill].transform.localPosition.x, 0.0f
                        , 0.0f), 0.5f);
                break;
            }
        }
    }
}
