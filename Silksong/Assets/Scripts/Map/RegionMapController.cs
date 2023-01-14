using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RegionMapController : MonoBehaviour
{
	private Dictionary<string, GameObject> mapIMGs = new Dictionary<string, GameObject>();
	private GameObject playerMarker, selectedRegion, currentRegion;

    void Awake()
    {
        playerMarker = transform.Find("PlayerMarker").gameObject;

        mapIMGs.Add("Level1", transform.Find("Region1").gameObject);
        mapIMGs.Add("Level3", transform.Find("Region2").gameObject);

        // selectedRegion = mapIMGs["Level1"];
    }

    // Update is called once per frame
    void Update()
    {
        //if (!EventSystem.current.currentSelectedGameObject) {
        //	Debug.Log("no selected object");
        //	EventSystem.current.SetSelectedGameObject(selectedRegion);
        //}

        //selectedRegion = EventSystem.current.currentSelectedGameObject;
        //EventSystem.current.SetSelectedGameObject(null);
        //EventSystem.current.SetSelectedGameObject(selectedRegion);

        //if (selectedRegion) {
	       // Vector3 c = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
	       // Vector3 d = c - selectedRegion.transform.position;
	       // foreach (KeyValuePair<string, GameObject> mapIMG in mapIMGs) {
	       // 	mapIMG.Value.transform.position += d;
	       // }
        //}

        //playerMarker.transform.position = currentRegion.transform.position;
    }

    public void SetCurrentRegion(string region)
    {
        Debug.Log("zzzzzzzzzzzz "  + region);
    	if (!mapIMGs.ContainsKey(region)) return;
    	currentRegion = mapIMGs[region];
    	selectedRegion = currentRegion;
    }
}
