using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SettingMenuManager : MonoBehaviour
{
    #region 调分辨率
    [HideInInspector] public Resolution[] Resolutions;
    private void Awake()
    {
        Resolutions = Screen.resolutions;
    }


    public string[] ResolutionStrings()
    {
        string[] sArr = new string[Resolutions.Length];
        for (var i = 0; i < Resolutions.Length; i++)
        {
            sArr[i] =  i + ": " + Resolutions[i].ToString();
        }

        return sArr;
    }

    public void SetFullScreen(bool isFullScreen)
    {
        if (isFullScreen)
        {
            Screen.fullScreen = true;
            Debug.Log("全屏");
        }
        else
        {
            Screen.fullScreen = false;
            Debug.Log("窗口化");
        }
    }

    public void SetResolution(int listIndex)
    {
        Resolution resolution = Resolutions[listIndex];
        Screen.SetResolution(resolution.width,resolution.height,Screen.fullScreen,resolution.refreshRate);
        Debug.Log("当前分辨率设置为 " + resolution);
    }


    #endregion

    #region 调音量

    public void ChangeVolume()
    {
        //AkSoundEngine.SetRTPCValue();
        //AkSoundEngine.
    }
    
    
    

    #endregion
    
    
    
    
    
    
    
    private int index = 0;
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 50), "change resolution++"))
        {
            index++;
            SetResolution(index);
        }
        
        if (GUI.Button(new Rect(200, 10, 100, 50), "change resolution--"))
        {
            index--;
            SetResolution(index);
        }
        
        if (GUI.Button(new Rect(400, 10, 100, 50), "change fullscreen"))
        {
            if (Screen.fullScreen)
            {
                SetFullScreen(false);
            }
            else
            {
                SetFullScreen(true);
            }
        }
        if (GUI.Button(new Rect(600, 10, 100, 50), "show all resolution"))
        {
            string[] s = ResolutionStrings();
            foreach (var s1 in s)
            {
                Debug.Log(s1);
            }
        }
    }

    
}

