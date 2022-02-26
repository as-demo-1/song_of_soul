using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMapController : MonoBehaviour
{
    private bool interactable;
    private float speed;
    private Dictionary<string, GameObject> mapIMGs = new Dictionary<string, GameObject>();
    private Transform canvas;
    private GameObject playerMarker;

    // Start is called before the first frame update
    void Start()
    {
        speed = 50.0f;
        interactable = false;
        canvas = transform.Find("Canvas");
        playerMarker = canvas.Find("PlayerMarker").gameObject;

        mapIMGs.Add("Level1", canvas.Find("Region1").gameObject);
        mapIMGs.Add("Level3", canvas.Find("Region2").gameObject);
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
    }

    public void SetInteractable(bool val)
    {
    	this.interactable = val;
    }
}
