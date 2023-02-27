using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clarinet : MonoBehaviour
{
    // Start is called before the first frame update
    //private float screenWidth;
    //private float screenHeight;
    private Animator animator;
    public string _chart;
    public GameObject spike;
    private Camera mainCamera;
    private int numCols; 
    void Start()
    {
        //transform.SetParent(null);
        numCols = 5;
        animator = GetComponent<Animator>();
        _chart = "10111";
    }

    public void Generate()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void Attack()
    {
        //this.gameObject.SetActive(true);
        if (this.transform.childCount <= 0)
        {
            //Generate(numCols);
        }
        for (int col = 0; col < numCols; ++col)
        {
            GameObject cellObject = transform.GetChild(col).gameObject;
            if (_chart[col] == '1')
            {
                cellObject.SetActive(true);
            }else
            {
                cellObject.SetActive(false);
            }
        }
        Debug.Log("before");
        animator.Play("up");
        Debug.Log("after");
    }

    public void End()
    {
        animator.Play("down");
    }
}
