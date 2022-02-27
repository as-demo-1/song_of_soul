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
