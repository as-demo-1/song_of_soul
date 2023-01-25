using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructiblePlatform : MonoBehaviour
{

    [SerializeField] private string _guid;
    [SerializeField] private SaveSystem _saveSystem;
    public string GUID => GetComponent<GuidComponent>().GetGuid().ToString();

    protected PlayerController playerController;
    protected Collider2D collider2d;

    public LayerMask targetLayer; //´¥·¢¸ÃtriggerµÄlayer
    public bool canWork;

    public int strengthToBreak;

    [DisplayOnly]
    public int playerPlungeStrength;


    private void OnValidate()
    {
        _guid = GUID;
    }


    protected void Start() {
        if (_saveSystem.ContainDestructiblePlatformGUID(_guid))
        {
            Destroy(this.gameObject);
            //gameObject.SetActive(false);
        }
        else
        {
            collider2d = GetComponent<Collider2D>();
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        if (canWork && targetLayer.Contains(collision.gameObject)) {

            playerController = collision.GetComponent<PlayerController>();

            playerPlungeStrength = (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Plunge] as PlayerPlunge).plungeStrength;

            // Debug.Log("PlungeStrength:" + playerPlungeStrength + " PlatformStrength:" + strengthToBreak);

            if(/*playerController.animatorParamsMapping.CurrentStatesParamHash == 130 &&*/ playerPlungeStrength >= strengthToBreak) {
                BreakThisPlatform();
            }
        }
    }


    public void BreakThisPlatform() {
        collider2d.enabled = false;
        playerController.setRigidVelocity(new Vector2(0, -1 * Constants.PlayerPlungeSpeed));

        _saveSystem.AddDestructiblePlatformGUID(_guid);
        
        // Destroy(this.gameObject);
        gameObject.SetActive(false);
    }


    private void OnTriggerExit2D(Collider2D collision) {
        if (canWork && targetLayer.Contains(collision.gameObject)) {
            playerController = null;
        }
    }

    /*
    protected void Update()
    {
        // Debug.Log(Application.persistentDataPath);

        if (Input.GetKeyDown(KeyCode.O))
        {
            _saveSystem.LoadSaveDataFromDisk();
        }
    }
    */
}
