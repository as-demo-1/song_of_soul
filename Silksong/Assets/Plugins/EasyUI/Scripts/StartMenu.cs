using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 开始菜单
/// </summary>
///
namespace EasyUI
{
    public class StartMenu : MonoBehaviour
    {
        public string startLevelName;
        // Start is called before the first frame update
        void Start()
        {
            
        }
    
        // Update is called once per frame
        void Update()
        {
            
        }
    
        public void OnClickStart()
        {
            SceneManager.LoadScene(startLevelName);
        }
    
        public void OnClickSet()
        {}
    
        public void OnClickQuit()
        {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
                Application.Quit();
    #endif
        }
    }
}

