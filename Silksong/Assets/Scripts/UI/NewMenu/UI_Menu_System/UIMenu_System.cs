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

	private void OnEnable()
	{
		PlayerInput.Instance.ReleaseControls();
	}
	void Start()
    {
        btn.SelectTab(0);
        MapController.Init();
		//PlayerInput.Instance.ReleaseControls();
	}

	public void ChangeTab(int index)
	{
		btn.SelectTab(index);
	}

    // Update is called once per frame
    void Update()
    {
    }

	private void OnDisable()
	{
		PlayerInput.Instance.GainControls();
	}
}
