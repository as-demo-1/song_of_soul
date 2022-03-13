using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTree_Pedal : MonoBehaviour
{
    public float CD;
    private bool isGrow = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //暂时的方案，等boss完成后替换为检测boss是否在场景中
        GameObject player = GameObject.Find("Player");
        if (player == null && isGrow == false)
        {
            StartCoroutine(GrowTree());
            isGrow = true;
        }

    }

    private IEnumerator GrowTree()
    {
        GameObject go = GameObject.Find("LightTree");
        List<Transform> lst = new List<Transform>();
        foreach (Transform child in go.transform)
        {
            lst.Add(child);
            Debug.Log(child.gameObject.name);
        }

        int i = 0;

        while (i < lst.Count)
        {
            yield return new WaitForSeconds(CD);//以CD为间隔长出平台
            Debug.Log("出现的平台是：" + lst[i].gameObject);
            lst[i].gameObject.layer = 6;
            i++;
        }
    }

}
