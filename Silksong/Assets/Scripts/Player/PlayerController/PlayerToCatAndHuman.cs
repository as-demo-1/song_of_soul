using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToCatAndHuman
{
    private bool isCat;
    public bool IsCat
    {
        get { return isCat; }
        set
        {
            isCat = value;
            playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.IsCatParamHas, value);
        }
    }

    private bool hasUpSpaceForHuman;
    public bool HasUpSpaceForHuman
    {
        get { return hasUpSpaceForHuman; }
        set
        {
            hasUpSpaceForHuman = value;
            playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.HasUpSpaceForHumanParamHas, value);
        }
    }


    private PlayerController playerController;
    public PlayerToCatAndHuman(PlayerController playerController)
    {
        this.playerController = playerController;
    }

    private Vector2 runStartPos;
    public bool isFastMoving;
    private float fastMoveStartAbsSpeed;
    public void toCat()
    {
        if (IsCat) return;

        IsCat = true;
        playerController.gameObject.layer = LayerMask.NameToLayer("PlayerCat");

        playerController.boxCollider.offset = new Vector2(playerController.boxCollider.offset.x, playerController.boxCollider.offset.y);
        playerController.boxCollider.size = new Vector2(Constants.playerCatBoxColliderWidth, Constants.playerCatBoxColliderHeight);

        playerController.groundCheckCollider.offset = new Vector2(playerController.groundCheckCollider.offset.x, Constants.playerCatGroundCheckColliderOffsetY);
        playerController.groundCheckCollider.size = new Vector2(Constants.playerCatBoxColliderWidth, playerController.groundCheckCollider.size.y);

        playerController.underWaterCheckCollider.radius = Constants.playerWaterCheckColliderRadiusCat;
    }

    public void colliderToHuman()
    {
        void checkIfNeedMoveAwayFromGround()//to prevent player from dropped in ground
        {
            Vector2 t = playerController.transform.position;
            RaycastHit2D hit = Physics2D.Raycast(t, Vector2.down, 100.0f, 1<<LayerMask.NameToLayer("Ground"));
            //Debug.Log(hit.distance);
            if (hit.distance>0.5f || hit.collider==null )
            {
                RaycastHit2D hitLeft = Physics2D.Raycast(t, new Vector2(-1,-1), 100.0f, 1 << LayerMask.NameToLayer("Ground"));
                RaycastHit2D hitRight = Physics2D.Raycast(t, new Vector2(1, -1), 100.0f, 1 << LayerMask.NameToLayer("Ground"));
     
                if(hitLeft.distance > 0.5f || hitLeft.collider == null)
                {
                   // Debug.Log("c to h left");//to do
                    playerController.transform.position = new Vector2(t.x + 0.3f , t.y + 0.4f);
                }
                else if(hitRight.distance > 0.5f || hitRight.collider == null)
                {
                    //Debug.Log("c to h right");//to do
                    playerController.transform.position = new Vector2(t.x -0.3f , t.y + 0.4f);
                }
                
            }
               
        }


        if (!IsCat) return;

        checkIfNeedMoveAwayFromGround();
        playerController.boxCollider.offset = new Vector2(playerController.boxCollider.offset.x, playerController.boxCollider.offset.y);
        playerController.boxCollider.size = new Vector2(Constants.playerBoxColliderWidth, Constants.playerBoxColliderHeight);

        playerController.groundCheckCollider.offset = new Vector2(playerController.groundCheckCollider.offset.x, Constants.playerGroundCheckColliderOffsetY);
        playerController.groundCheckCollider.size = new Vector2(Constants.playerGroundCheckColliderSizeX, playerController.groundCheckCollider.size.y);

        playerController.underWaterCheckCollider.radius = Constants.playerWaterCheckColliderRadius;
    }

    public void stateToHuman()
    {
        if (!IsCat) return;

        IsCat = false;
        isFastMoving = false;
        playerController.gameObject.layer = LayerMask.NameToLayer("Player");

    }
    public void toHuman()
    {
        if (isCat == false) return;
        colliderToHuman();
        stateToHuman();
    }
    //------------------------------------------------------------------
    public void catUpdate()
    {
        if (!IsCat) return;

        checkUpSpaceForHuman();
        checkFastMoveEnd();
    }
    private void checkUpSpaceForHuman()
    {

        Vector2 distance = new Vector2(0.125f, 0.5f);
        Vector2 YOffset = new Vector2(0, 0.5f);

        Vector2 upPoint = (Vector2)playerController.transform.position + YOffset;
        Vector2 upPointA = upPoint + distance;
        Vector2 upPointB = upPoint - distance;
        // Debug.DrawLine(upPointA, upPointB);

        Vector2 downPoint = (Vector2)playerController.transform.position - YOffset;
        Vector2 downPointA = downPoint + distance;
        Vector2 downPointB = downPoint - distance;
        // Debug.DrawLine(downPointA, downPointB);
        //Debug.Log(Physics2D.OverlapArea(upPointA, upPointB, LayerMask.NameToLayer("Ground")));
        if (Physics2D.OverlapArea(upPointA, upPointB, 1 << LayerMask.NameToLayer("Ground")) && Physics2D.OverlapArea(downPointA, downPointB, 1 << LayerMask.NameToLayer("Ground")))
        {
            HasUpSpaceForHuman = false;
        }
        else
        {
            HasUpSpaceForHuman = true;
        }
    }

    public void moveDistanceCount()
    {
        if (!IsCat) return;

        if (!isFastMoving && Mathf.Abs(playerController.transform.position.x - runStartPos.x) > Constants.PlayerCatToFastMoveDistance)
        {
            isFastMoving = true;
            Debug.Log("cat fast move");
            fastMoveStartAbsSpeed = Mathf.Abs(playerController.getRigidVelocity().x);
        }
    }

    private void checkFastMoveEnd()
    {
        if (isFastMoving && Mathf.Abs(playerController.getRigidVelocity().x) < fastMoveStartAbsSpeed)
        {
            isFastMoving = false;
            Debug.Log("cat fast end");
            /* Debug.Log(playerController.getRigidVelocity().x);
             Debug.Log(fastMoveDir);
             Debug.Log(playerController.getRigidVelocity().x != fastMoveDir);*/
        }
    }
    public void catMoveStart()
    {
        if (!IsCat) return;
        runStartPos = playerController.transform.position;
    }

    public void extraJump()
    {
        if (!isFastMoving || playerController.isGroundedBuffer()) return;

        playerController.PlayerAnimator.Play("CatToHumanExtraJump");
        Debug.Log("extra jump");
        float speed = Mathf.Sqrt(Physics2D.gravity.y * -1 * Constants.PlayerNormalGravityScale * 2 * Constants.PlayerCatToHumanExtraJumpHeight);
        playerController.setRigidVelocity(new Vector2(playerController.getRigidVelocity().x, speed));
    }


}
