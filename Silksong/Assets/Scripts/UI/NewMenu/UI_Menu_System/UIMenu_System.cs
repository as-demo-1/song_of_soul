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

	private void OnEnable()
	{
		//PlayerInput.Instance.ReleaseControls();
	}
	void Start()
    {
        btn.SelectTab(0);
		PlayerInput.Instance.ReleaseControls();
	}

    // Update is called once per frame
    void Update()
    {
    }

	private void OnDestroy()
	{
		PlayerInput.Instance.GainControls();
	}
}
