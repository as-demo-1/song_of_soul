using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapController : MonoBehaviour
{
    public static MapController Instance
    {
        get { return s_Instance; }
    }

    protected static MapController s_Instance;

	[SerializeField] public GameObject levelMap;
	[SerializeField] public GameObject regionMap;
    [SerializeField] public GameObject mapUI;
	private LevelMapController levelMapController;
	private RegionMapController regionMapController;
    private MapUIController mapUIController;

	private bool quick, showLevel;
    private string region;

    // Start is called before the first frame update
    void Start()
    {
    	quick = true;
    	levelMapController = levelMap.GetComponent<LevelMapController>();
    	regionMapController = regionMap.GetComponent<RegionMapController>();
        mapUIController = mapUI.GetComponent<MapUIController>();
    	levelMap.SetActive(false);
    	regionMap.SetActive(false);
        mapUI.SetActive(false);
    }

    void Update()
    {
        region = SceneManager.GetActiveScene().name.Split('-')[0];

    	// show quick map
    	if (quick && PlayerInput.Instance.quickMap.Down) {
    		levelMap.SetActive(true);
    		levelMapController.SetInteractable(false);
            levelMapController.centering(region);
            PlayerAnimatorParamsMapping.SetControl(false);
    	}

    	// hide quick map
    	if (quick && PlayerInput.Instance.quickMap.Up) {
    		levelMap.SetActive(false);
            PlayerAnimatorParamsMapping.SetControl(true);
        }

    	// show map
    	if (PlayerInput.Instance.showMap.Down) {
    		quick = false;
            showLevel = false;
            levelMap.SetActive(false);
            regionMap.SetActive(true);
            mapUI.SetActive(true);
            regionMapController.SetCurrentRegion(region);
            mapUIController.showLevelMapIns(false);
            PlayerAnimatorParamsMapping.SetControl(false);
        }

    	if (!quick) {
    		// hide all maps
    		if (Input.GetButtonDown("Cancel")) {
                if (showLevel) {
                    switch2RegionMap();
                }
                else {
                    quick = true;
                    levelMap.SetActive(false);
                    regionMap.SetActive(false);
                    mapUI.SetActive(false);
                    PlayerAnimatorParamsMapping.SetControl(true);
                }
    		}
    		// show region map
    		if (PlayerInput.Instance.jump.Down) {
                switch2RegionMap();
    		}
    	}
    }

    public void switch2LevelMap(GameObject levelMapIMG)
    {
        showLevel = true;
        regionMap.SetActive(false);
        levelMap.SetActive(true);
        levelMapController.SetInteractable(true);
        levelMapController.centering(levelMapIMG);
        mapUIController.showLevelMapIns(true);
    }

    void switch2RegionMap()
    {
        showLevel = false;
    	levelMap.SetActive(false);
    	regionMap.SetActive(true);
        mapUIController.showLevelMapIns(false);
    }
}
