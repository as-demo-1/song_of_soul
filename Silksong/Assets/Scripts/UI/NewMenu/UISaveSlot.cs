using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISaveSlot : MonoBehaviour, ISelectHandler
{
    public UISaveView uiSaveView;
    public int slot;

    public Image image;
    public Text Name;
    public Text time;
    public Text allTime;

    public Button saveBtn;
    public Button loadBtn;

    // Start is called before the first frame update

    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {


	}  
    
    public void Init(Save save)
    {
        image.gameObject.SetActive(true);

		Name.text = save.levelName;
        time.text = "保存时间" + DateTime.FromFileTime(save.timestamp);
        time.gameObject.SetActive(true);
        allTime.text = " 游玩时间";
        allTime.gameObject.SetActive(true);
    }

	public void OnSelect(BaseEventData eventData)
	{
		//uiSaveView.index = slot;
	}
}
