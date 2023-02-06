using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCharm : MonoBehaviour
{
    public BuffManager buffManager;
    public string testCharmBuffID;
    public string testBuffVal;

    public CharmBuff charmBuff;
    // Start is called before the first frame update
    void Start()
    {
        buffManager.AddBuff(testCharmBuffID, testBuffVal);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
