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
    private GameObject panel;
    public KeyCode ActionKey;
    public KeyCode CloseKey;

    private bool isEnter;
    //string guid;

    private void Update()
    {
        if (isEnter)
        {
            if (Input.GetKeyUp(ActionKey))
            {
                if (panel==null)
                {
                    panel = UIManager.Instance.Show<UISaveView>().gameObject;
                    panel.GetComponent<UISaveView>().backBtn.onClick.AddListener(() => PlayerInput.Instance.GainControls());
                }
                else
                {
                    panel.SetActive(true);
                }
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
    }

    private void Awake()
    {
        //SaveSystem _saveSystem = GameManager.Instance.saveSystem;
        //guid = GetComponent<GuidComponent>().GetGuid().ToString();
        //panel.SetActive(false);
        tip.SetActive(false);
        isEnter = false;
    }

    protected override void enterEvent()
    {
        tip.SetActive(true);
        isEnter = true;
    }
    

    protected override void exitEvent()
    {
        tip.SetActive(false);
        isEnter = false;
    }
}
