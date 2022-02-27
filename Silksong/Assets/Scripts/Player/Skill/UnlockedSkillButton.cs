using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockedSkillButton : MonoBehaviour
{
    public Action EquipSkill;


    private void Start()
    {
        GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnEquipSkill);
    }

    private void OnEquipSkill()
    {
        EquipSkill.Invoke();
    }

}
