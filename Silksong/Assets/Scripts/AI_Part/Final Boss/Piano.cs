using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piano : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private string[] chartList;
    [SerializeField] private int currentChart;
    private int numCols; 

    private void Awake()
    {
        numCols = this.transform.childCount;
    }


    public void Generate()
    {
        for (int col = 0; col < numCols; col++)
        {
            this.transform.GetChild(col).GetChild(chartList[currentChart][col] - '0').GetChild(0).gameObject.SetActive(true);
        }
    }

    
    public void Attack()
    {
        for (int col = 0; col < numCols; ++col)
        {
            this.transform.GetChild(col).GetChild(chartList[currentChart][col] - '0').GetChild(1).gameObject.SetActive(true);
        }
    }
    
    public void End()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            for (int col = 0; col < numCols; col++)
            {
                this.transform.GetChild(col).GetChild(chartList[currentChart][col] - '0').GetChild(1).gameObject.SetActive(false);
                this.transform.GetChild(col).GetChild(chartList[currentChart][col] - '0').GetChild(2).gameObject.SetActive(true);
            }
        }
    }
}
