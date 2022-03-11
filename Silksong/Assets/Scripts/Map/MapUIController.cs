using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUIController : MonoBehaviour
{
	private GameObject levelMapIns, regionMapIns;

    void Awake()
    {
        levelMapIns = transform.Find("LevelMapInstruction").gameObject;
        regionMapIns = transform.Find("RegionMapInstruction").gameObject;
    }

    public void showLevelMapIns(bool val)
    {
    	levelMapIns.SetActive(val);
    	regionMapIns.SetActive(!val);
    }

}
