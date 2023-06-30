using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PanelView))]
public class SwitchSoulSkill : MonoBehaviour
{
    private PanelView _panelView;
    // Start is called before the first frame update
    void Start()
    {
        _panelView = GetComponent<PanelView>();
        _panelView.OnPanelChange.AddListener(ChangeSoulSkill);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ChangeSoulSkill()
    {
        PlayerController.Instance.SoulSkillController.ChangeEquip(
            (SkillName)_panelView.SelectIndex);
    }
}
