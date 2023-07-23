using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TutorialTrigger : Trigger2DBase
{
	[SerializeField] private GameObject panel;

    private bool hasTriggered;

    //[SerializeField] private TutorialPanelSO TutorialPanelSO;

	private void Awake()
	{
		panel.SetActive(false);
	}

	protected override void enterEvent()
	{
		//Debug.Log("Trigger");
		
		panel.SetActive(true);
		//panel.transform.DOShakeScale(0.5f, 0.2f);
		hasTriggered = true;
		//TutorialPanelSO.RasieEvent(title);
		PlayerInput.Instance.ReleaseControls();// 禁用玩家输入
		Debug.Log("Player Trigger Tutorial");
		
	}
	
	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.J) && hasTriggered)
		{
			panel.SetActive(false);
			PlayerInput.Instance.GainControls();
			//this.enabled = false;
		}
	}
}
