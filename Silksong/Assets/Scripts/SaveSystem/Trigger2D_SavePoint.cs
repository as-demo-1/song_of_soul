 using System;
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using UnityEngine.Serialization;

 /// <summary>
 /// 存档点触发区
 /// </summary>
public class Trigger2D_SavePoint : Trigger2DBase
 {
     public GameObject tip;
    public GameObject panel;
    public KeyCode ActionKey;
    public KeyCode CloseKey;
    //string guid;

    private void Update()
    {

    }

    private void Awake()
    {
        //SaveSystem _saveSystem = GameManager.Instance.saveSystem;
        //guid = GetComponent<GuidComponent>().GetGuid().ToString();
        //panel.SetActive(false);
        tip.SetActive(false);
    }

    protected override void enterEvent()
    {
        tip.SetActive(true);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        if (Input.GetKeyUp(ActionKey))
        {
            //panel.SetActive(true);
            UIManager.Instance.Show<UISaveView>().backBtn.onClick
                .AddListener(() => PlayerInput.Instance.GainControls());
            tip.SetActive(false);
            PlayerInput.Instance.ReleaseControls();
        }
        
        if (Input.GetKeyUp(CloseKey))
        {
            //panel.SetActive(true);
            UIManager.Instance.Close<UISaveView>();
            tip.SetActive(true);
            PlayerInput.Instance.GainControls();
        }
        
    }

    protected override void exitEvent()
    {
        tip.SetActive(false);
    }
}
