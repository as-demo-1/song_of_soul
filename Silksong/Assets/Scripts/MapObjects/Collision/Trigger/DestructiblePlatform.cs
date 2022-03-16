using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructiblePlatform : MonoBehaviour
{
    protected PlayerController playerController;
    protected Collider2D collider2d;

    public LayerMask targetLayer; //´¥·¢¸ÃtriggerµÄlayer
    public bool canWork;

    public int strengthToBreak;


    [DisplayOnly]
    public int playerPlungeStrength;

    protected void Start() {
        collider2d = GetComponent<Collider2D>();
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        if (canWork && targetLayer.Contains(collision.gameObject)) {

            playerController = collision.GetComponent<PlayerController>();

            playerPlungeStrength = playerController.playerStatesBehaviour.playerPlunge.plungeStrength;

            Debug.Log("PlungeStrength:" + playerPlungeStrength + " PlatformStrength:" + strengthToBreak);

            if(playerPlungeStrength >= strengthToBreak) {
                // playerController.playerStatesBehaviour.playerPlunge.willBreakGround = true;
                BreakThisPlatform();
            }

            else {
                // playerController.playerStatesBehaviour.playerPlunge.willBreakGround = false;
            }

        }
    }


    public void BreakThisPlatform() {
        // playerController.playerStatesBehaviour.playerPlunge.willBreakGround = false;
        collider2d.enabled = false;        
        playerController.setRigidVelocity(new Vector2(0, -1 * playerController.playerInfo.plungeSpeed));

        Destroy(this.gameObject);

        // Debug.Log("break platform");

    }


    private void OnTriggerExit2D(Collider2D collision) {
        if (canWork && targetLayer.Contains(collision.gameObject)) {
            playerController = null;
        }
    }

}
