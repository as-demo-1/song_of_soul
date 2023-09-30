using System;
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

	//[SerializeField] public GameObject levelMap;
	//[SerializeField] public GameObject regionMap;
    //[SerializeField] public GameObject mapUI;
    public LevelMapController levelMapController;
    public RegionMapController regionMapController;
    //public MapUIController mapUIController;

    public bool quick, showLevel;
    [SerializeField] private string region;

    public RectTransform levelRoot;
    private Vector3 movement;

    // Start is called before the first frame update
    public void Init()
    {
    	quick = true;
    	
    	Debug.Log("map init");
        //mapUIController = mapUI.GetComponent<MapUIController>();
        levelMapController.gameObject.SetActive(true);
        regionMapController.gameObject.SetActive(false);
        ShowLevel();
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

        if (showLevel)
        {
            movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f);
        }
    }

    private void FixedUpdate()
    {
        if (showLevel)
        {
            levelRoot.position +=  movement * Time.fixedDeltaTime*200.0f;
        }
    }

    private void OnEnable()
    {
        ShowLevel();
    }

    public void ShowLevel()
    {
        // 根据主角所在位置设定地图位置
        levelRoot.position += 
            (Vector3)(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f) -
                      (Vector2)levelMapController.playerMarker.transform.position);
        
        //quick = true;
        showLevel = true;
        string sceneName = SceneManager.GetActiveScene().name;//.name.Split('-');
        
        
        levelMapController.gameObject.SetActive(true);
        levelMapController.SetCurrentLevel(sceneName);
        
        regionMapController.gameObject.SetActive(false);
        //levelMapController.SetInteractable(false);
        //levelMapController.centering(region);
        //PlayerAnimatorParamsMapping.SetControl(false);
    }

    public void ShowRegion()
    {
        //quick = false;
        showLevel = false;
        levelMapController.gameObject.SetActive(false);
        regionMapController.gameObject.SetActive(true);
        //levelMapController.SetInteractable(true);
        region = SceneManager.GetActiveScene().name.Split('-')[0];
        //mapUI.SetActive(true);
        regionMapController.SetCurrentRegion(region);
        //mapUIController.showLevelMapIns(false);
        //PlayerAnimatorParamsMapping.SetControl(false);
    }

    // public void switch2LevelMap(GameObject levelMapIMG)
    // {
    //     showLevel = true;
    //     levelMapController.gameObject.SetActive(true);
    //     regionMapController.gameObject.SetActive(false);
    //     levelMapController.SetInteractable(true);
    //     levelMapController.centering(levelMapIMG);
    //     //mapUIController.showLevelMapIns(true);
    // }

    // void switch2RegionMap()
    // {
    //     showLevel = false;
    //     levelMapController.gameObject.SetActive(false);
    //     regionMapController.gameObject.SetActive(true);
    //     //mapUIController.showLevelMapIns(false);
    // }
}
