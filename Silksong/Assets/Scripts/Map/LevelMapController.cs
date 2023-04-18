using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMapController : MonoBehaviour
{
    private bool interactable;
    private float speed;
    private Dictionary<string, GameObject> mapIMGs = new Dictionary<string, GameObject>();
    private GameObject sceneBound, mapBound, playerMarker;
    private string currentLevel;

    // Start is called before the first frame update
    void Start()
    {
        speed = 50.0f;
        interactable = false;
        playerMarker = transform.Find("PlayerMarker").gameObject;

        mapIMGs.Add("Level1", transform.Find("Region1").gameObject);
        mapIMGs.Add("Level3", transform.Find("Region2").gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (interactable) {
			float t = Time.smoothDeltaTime;
			Vector3 d = new Vector3(PlayerInput.Instance.horizontal.Value * t * speed, PlayerInput.Instance.vertical.Value * t * speed, 0f);
			foreach (KeyValuePair<string, GameObject> mapIMG in mapIMGs) {
				mapIMG.Value.transform.position += d;
			}
        }

        // trace player position
        string level = SceneManager.GetActiveScene().name;

        if (level != currentLevel) {
            currentLevel = level;
            sceneBound = GameObject.Find(level + "_SceneBound");
            string region = level.Split('-')[0];
            if (mapIMGs.ContainsKey(region)) {
                mapBound = mapIMGs[region].transform.Find(level).gameObject;
            }
        }
        
        if (sceneBound && mapBound) {
            Vector3 p = new Vector3();
            p = PlayerInput.Instance.transform.position - sceneBound.transform.position;

            RectTransform mapBoundRectTrans = mapBound.transform.GetComponent<RectTransform>();
            RectTransform sceneBoundRectTrans = sceneBound.transform.GetComponent<RectTransform>();
            p.x *= mapBoundRectTrans.rect.width * mapBoundRectTrans.lossyScale.x / (sceneBoundRectTrans.rect.width * sceneBoundRectTrans.lossyScale.x);
            p.y *= mapBoundRectTrans.rect.height * mapBoundRectTrans.lossyScale.y / (sceneBoundRectTrans.rect.height * sceneBoundRectTrans.lossyScale.y);
            playerMarker.transform.position = mapBound.transform.position + p;

            // Debug.Log("sceneBound" + sceneBound.transform.position + " mapBound" + mapBound.transform.position + " playerPos" + PlayerInput.Instance.transform.position + " p" + p + " sceneDim" + sceneBound.transform.localScale + " mapDim" + mapBoundRectTrans.rect);
        }
    }

    // centering the map image with key value region
    public void centering(string region)
    {
        if (!mapIMGs.ContainsKey(region)) return;
        Vector3 c = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        Vector3 d = c - mapIMGs[region].transform.position;
        foreach (KeyValuePair<string, GameObject> mapIMG in mapIMGs) {
            mapIMG.Value.transform.position += d;
        }
    }

    // centering the map image object m
    public void centering(GameObject m)
    {
        Vector3 c = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        Vector3 d = c - m.transform.position;
        foreach (KeyValuePair<string, GameObject> mapIMG in mapIMGs) {
            mapIMG.Value.transform.position += d;
        }
    }

    public void SetInteractable(bool val)
    {
    	this.interactable = val;
    }
}
