using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using UnityEngine.Events;

public class OnSureWindow : Window
{
    string mTitle;
    string mText;
    EventCallback0 onSure;
    EventCallback0 onCancel;

    public OnSureWindow(string title, string text, EventCallback0 onSure, EventCallback0 onCancel)
    {
        mTitle = title;
        mText = text;
        this.onSure = onSure;
        this.onCancel = onCancel;
    }

    protected override void OnInit()
    {
        this.contentPane = UIPackage.CreateObject("OnSureMenu", "OnSureMenu").asCom;
        this.Center();
        this.modal = true;

        GTextField gTitle = contentPane.GetChild("title").asTextField;
        gTitle.text = mTitle;
        GTextField gText = contentPane.GetChild("text").asTextField;
        gText.text = $"    {mText}";
        GButton gBtn_sure = contentPane.GetChild("btn_sure").asButton;
        gBtn_sure.GetChild("title").asTextField.text = "确认";
        gBtn_sure.onClick.Add(onSure);
        GButton gBtn_cancel = contentPane.GetChild("btn_cancel").asButton;
        gBtn_cancel.GetChild("title").asTextField.text = "取消";
        gBtn_cancel.onClick.Add(onCancel);
    }
}
