using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SeasonsDoor : MonoBehaviour
{

    [SerializeField]private string NextMapName;

    public void LoadNextMap() //暂时方案...
    {
        SceneManager.LoadScene(NextMapName);
    }
}
//季节门直接就没完工啊，这个code代表的是我们现有的场景切换。。。