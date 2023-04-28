using BehaviorDesigner.Runtime.Tasks.Unity.UnityInput;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabBtn : MonoBehaviour//, ISelectHandler, IDeselectHandler
{

	public bool isSelected;
	public GameObject selectedBg;
	public GameObject selected;
	public GameObject normalBg;

	public void ChangeBg(bool i)
	{
		if (selected != null)
		{
			selected.SetActive(i);
		}

		if (selectedBg != null)
		{
			selectedBg.SetActive(i);
		}

		if (normalBg != null)
		{
			normalBg.SetActive(!i);
		}
	}

	/*public void OnDeselect(BaseEventData eventData)
	{
		isSelected = false;
	}

	public void OnSelect(BaseEventData eventData)
	{
		tabView.SelectTab(tabIndex);
		isSelected = true;
	}*/
}
