using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SongExtendPlatform : MonoBehaviour, SingComponent
{
    public List<GameObject> platforms;
    public void Start()
    {
        SceneManager.activeSceneChanged += (arg0, arg1) =>
        {
            foreach (var a in platforms) a.SetActive(false);
        };    
    }
    public void WhenSinging()
    {
        for(int i = 0; i < platforms.Count; i++)
        {
            platforms[i].SetActive(true);
        }
    }
    
}
