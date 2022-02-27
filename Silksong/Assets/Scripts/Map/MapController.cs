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
	private LevelMapController levelMapController;
	private RegionMapController regionMapController;

	private bool quick;
    private string region;

    // Start is called before the first frame update
    void Start()
    {
    	quick = true;
    	levelMapController = FindObjectOfType<LevelMapController>();
    	regionMapController = FindObjectOfType<RegionMapController>();
    	levelMap.SetActive(false);
    	regionMap.SetActive(false);
    }

    void Update()
    {
        string region = SceneManager.GetActiveScene().name.Split('-')[0];

    	// show quick map
    	if (quick && PlayerInput.Instance.quickMap.Down) {
    		levelMap.SetActive(true);
    		levelMapController.SetInteractable(false);
            levelMapController.centering(region);
    	}

    	// hide quick map
    	if (quick && PlayerInput.Instance.quickMap.Up) {
    		levelMap.SetActive(false);
    	}

    	// show map
    	if (PlayerInput.Instance.showMap.Down) {
    		quick = false;
    		levelMap.SetActive(true);
    		levelMapController.SetInteractable(true);
            levelMapController.centering(region);
    	}

    	if (!quick) {
    		// hide all maps
    		if (Input.GetButtonDown("Cancel")) {
    			quick = true;
    			levelMap.SetActive(false);
    			regionMap.SetActive(false);
    		}
    		// show level map
    		// if (PlayerInput.Instance.normalAttack.Down) {
    		// 	regionMap.SetActive(false);
    		// 	levelMap.SetActive(true);
    		// }
    		// show region map
    		if (PlayerInput.Instance.jump.Down) {
    			levelMap.SetActive(false);
    			regionMap.SetActive(true);
    		}
    	}
    }

    void switch2LevelMap(GameObject levelMapIMG)
    {

    }

    void switch2RegionMap()
    {
    	levelMap.SetActive(false);
    	regionMap.SetActive(true);
    }
}
