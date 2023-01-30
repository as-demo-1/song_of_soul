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
    //[SerializeField] public GameObject mapUI;
    public LevelMapController levelMapController;
    public RegionMapController regionMapController;
    //public MapUIController mapUIController;

    public bool quick, showLevel;
    private string region;

    // Start is called before the first frame update
    public void Init()
    {
    	quick = true;
    	levelMapController = levelMap.GetComponent<LevelMapController>();
    	regionMapController = regionMap.GetComponent<RegionMapController>();
        //mapUIController = mapUI.GetComponent<MapUIController>();
    	levelMap.SetActive(false);
    	regionMap.SetActive(false);
        //mapUI.SetActive(false);
    }

    void Update()
    {
    	if (PlayerInput.Instance.showMap.Down) {
            ShowRegion();
        }
        if (PlayerInput.Instance.showMap.Up)
        {
            ShowLevel();
        }
    }

    public void ShowLevel()
    {
        quick = true;
        showLevel = true;
        region = SceneManager.GetActiveScene().name.Split('-')[0];
        levelMap.SetActive(true);
        regionMap.SetActive(false);
        levelMapController.SetInteractable(false);
        //levelMapController.centering(region);
        PlayerAnimatorParamsMapping.SetControl(false);
    }

    public void ShowRegion()
    {
        quick = false;
        showLevel = false;
        levelMap.SetActive(false);
        regionMap.SetActive(true);
        levelMapController.SetInteractable(true);
        region = SceneManager.GetActiveScene().name.Split('-')[0];
        //mapUI.SetActive(true);
        //regionMapController.SetCurrentRegion(region);
        //mapUIController.showLevelMapIns(false);
        PlayerAnimatorParamsMapping.SetControl(false);
    }

    public void switch2LevelMap(GameObject levelMapIMG)
    {
        showLevel = true;
        regionMap.SetActive(false);
        levelMap.SetActive(true);
        levelMapController.SetInteractable(true);
        levelMapController.centering(levelMapIMG);
        //mapUIController.showLevelMapIns(true);
    }

    void switch2RegionMap()
    {
        showLevel = false;
    	levelMap.SetActive(false);
    	regionMap.SetActive(true);
        //mapUIController.showLevelMapIns(false);
    }
}
