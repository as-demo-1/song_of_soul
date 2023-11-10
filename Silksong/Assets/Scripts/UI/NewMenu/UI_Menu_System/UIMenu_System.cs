using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMenu_System : MonoBehaviour
{

    public bool isFirst;
    public bool isSecond;
    public bool isThird;
	public TabView btn;
	// Start is called before the first frame update

	public MapController MapController;
	public UIEquipView equipView;
	public UIQuestView questView;

	private void OnEnable()
	{
		equipView.Init();
		questView.Init();
	}
	void Start()
    {
	    DontDestroyOnLoad(this);
        btn.SelectTab(0);
        MapController.Init();
	}

	public void ChangeTab(int index)
	{
		btn.SelectTab(index);
	}

    // Update is called once per frame
    void Update()
    {
    }
}
