using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class DestructiblePlatform : MonoBehaviour
{

    public string GUID => GetComponent<GuidComponent>().GetGuid().ToString();

    protected PlayerController playerController;
    protected Collider2D collider2d;

    public LayerMask targetLayer; //´¥·¢¸ÃtriggerµÄlayer
    public bool canWork;

    public int strengthToBreak;


    public int playerPlungeStrength;




    protected void Start() {
        collider2d = GetComponent<Collider2D>();
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        if (canWork && collision.gameObject.tag=="PlayerGroundCheck") {

            playerController = collision.GetComponentInParent<PlayerController>();
            playerPlungeStrength = (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Plunge] as PlayerPlunge).plungeStrength;
            //Debug.Log(playerPlungeStrength);
            if( playerPlungeStrength >= strengthToBreak) {
                BreakThisPlatform();
            }
        }
    }


    public void BreakThisPlatform() {
        collider2d.enabled = false;
        playerController.setRigidVelocity(new Vector2(0, -1 * Constants.PlayerPlungeSpeed));
        Destroyed_StableSave stableSave;
        if (TryGetComponent(out stableSave) && !stableSave.ban)
        {
            stableSave.saveGamingData(true);
        }
        gameObject.SetActive(false);
    }


    private void OnTriggerExit2D(Collider2D collision) {
        if (canWork && targetLayer.Contains(collision.gameObject)) {
            playerController = null;
        }
    }

    
}
