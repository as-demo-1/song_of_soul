using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NPCController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    void Update()
    {
        
    }

    //todo: 阿草在对话结束后跑到屏幕外
    public void ACaoRun()
    {

    }

    public Vector3 GetTalkCoord()
    {
        InteractiveSO InteractiveItem = GetComponent<InteractiveObjectTrigger>().InteractiveItem;
        if (InteractiveItem.ItemType == EInteractiveItemType.DIALOG)
        {
            return transform.position + (InteractiveItem as NPCSO).TalkCoord;
        }
        else
        {
            throw new UnityException("该物体没有对话坐标");
        }
    }
}


