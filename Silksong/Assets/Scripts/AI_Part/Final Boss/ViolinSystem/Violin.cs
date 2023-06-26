using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Violin : MonoBehaviour
{
    private void Awake()
    {

    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attack()
    {
        this.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void End()
    {
        this.transform.GetChild(0).gameObject.SetActive(false);
    }
}
