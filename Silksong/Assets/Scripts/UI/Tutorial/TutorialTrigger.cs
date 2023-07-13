using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private string tagToDetect = "Player";
    [SerializeField] private GameObject panel;

    private bool hasTriggered;
    
	//[SerializeField] private TutorialPanelSO TutorialPanelSO;

	private void Awake()
	{
		panel.SetActive(false);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		//Debug.Log("Trigger");
		if (other.CompareTag(tagToDetect))
		{
			panel.SetActive(true);
			//panel.transform.DOShakeScale(0.5f, 0.2f);
			hasTriggered = true;
			//TutorialPanelSO.RasieEvent(title);
			PlayerInput.Instance.ReleaseControls();// Ω˚”√ÕÊº“ ‰»Î
			Debug.Log("Player Trigger Tutorial");
		}

	}
	/*private void OnTriggerExit2D(Collider2D other)
	{
		//Debug.Log("Trigger");
		if (other.CompareTag(tagToDetect))
		{
			panel.SetActive(false);
			//TutorialPanelSO.RasieEvent(title);
			Debug.Log("Player leave Tutorial");
		}

	}*/
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
