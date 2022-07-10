using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMessageBox : MonoBehaviour
{
    public Text title;
    public Text message;
    public Image[] icons;
    public Button buttonYes;
    public Button buttonNo;
    public Button buttonClose;

    public Text buttonYesTitle;
    public Text buttonNoTitle;

    public UnityAction OnYes;
    public UnityAction OnNo;


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(string title, string message, MessageBoxType type = MessageBoxType.Information, string btnOK = "", string btnCancel = "")
    {
        if (!string.IsNullOrEmpty(title)) this.title.text = title;
        this.message.text = message;
        this.icons[0].enabled = type == MessageBoxType.Information;
        this.icons[1].enabled = type == MessageBoxType.Confirm;
        this.icons[2].enabled = type == MessageBoxType.Error;

        if (!string.IsNullOrEmpty(btnOK)) this.buttonYesTitle.text = title;
        if (!string.IsNullOrEmpty(btnCancel)) this.buttonNoTitle.text = title;

        this.buttonYes.onClick.AddListener(OnClickYes);
        this.buttonNo.onClick.AddListener(OnClickNo);

        this.buttonNo.gameObject.SetActive(type == MessageBoxType.Confirm);
    }

    void OnClickYes()
    {
        Destroy(this.gameObject);
        if (this.OnYes != null)
            this.OnYes();
    }

    void OnClickNo()
    {
        Destroy(this.gameObject);
        if (this.OnNo != null)
            this.OnNo();
    }
}
