using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SeasonsDoor : MonoBehaviour
{

    [SerializeField]private string NextMapName;

    public void LoadNextMap()
    {
        SceneManager.LoadScene(NextMapName);
    }
}
