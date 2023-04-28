using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISaveSlot : MonoBehaviour, ISelectHandler
{
    public UISaveView uiSaveView;
    public int slot;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void OnSelect(BaseEventData eventData)
	{
		//uiSaveView.index = slot;
	}
}
