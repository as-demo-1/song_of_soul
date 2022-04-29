using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class DoorCtrl : MonoBehaviour
{
    public Animator DoorAnim;
    public BoxCollider2D Box;
    public SpriteRenderer talkImg;
    public Sprite[] pic;
    private bool isLock = false;
    private bool isPlayer = false;
    // Start is called before the first frame update
    void Start()
    {
        InitDoor();
    }

    void Update()
    {
        if (isPlayer && Input.GetKeyDown(KeyCode.F))
        {   
            OpenDoor();
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isLock)
        {   
            talkImg.enabled = true;
            isPlayer = true;
            if (KeyManager.Instance.GetKeyNum() > 0)
            {
                talkImg.sprite = pic[0];
            }
            else
            {
                talkImg.sprite = pic[1];
            }
           
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            talkImg.enabled = false;
            isPlayer = false;
        }
    }



    void InitDoor(){
        talkImg.enabled = false;
        Box.enabled = true;
        DoorAnim.enabled = false;
        
    }

    void OpenDoor() {
        if (KeyManager.Instance.GetKeyNum() > 0)
        {
            DoorAnim.enabled = true;
            Box.enabled = false;
            isLock = true;
            talkImg.enabled = false;
            isPlayer = false;
            EventManager.Instance.Dispatch<int>(EventType.onKeyChange, -1);
        }
    }
    
    
}
