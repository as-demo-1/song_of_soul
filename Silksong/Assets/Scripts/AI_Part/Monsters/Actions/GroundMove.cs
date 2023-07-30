using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Monsters")]
public class GroundMove : Action 
{
    public enum Direction {
        Left,
        Right
    }
    public Direction direction;
    public float speed = 100.0f;
    public enum EdgeHandle
    {
        Turn,
        Fall
    }
    public EdgeHandle edgeHandle= EdgeHandle.Turn;

    
    private Rigidbody2D rigidbody2D;
    private Collider2D collider2D;

    public override void OnAwake()
    {
        rigidbody2D = this.GetComponent<Rigidbody2D>();
        collider2D = this.GetComponent<Collider2D>();
        if (direction == Direction.Left)
        {
            this.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (hitGround()) {
            turnDirection();
        }

        if (nearPlatformBoundary())
        {
            if (edgeHandle == EdgeHandle.Turn)
            {
                turnDirection();
            }
        }

        if (direction == Direction.Left)
        {
            rigidbody2D.velocity = speed * Vector2.left * Time.deltaTime;
        } else
        {
            rigidbody2D.velocity = speed * Vector2.right * Time.deltaTime;
        }
        return TaskStatus.Running;
    }

    private void turnDirection()
    {
        if (direction == Direction.Left)
        {
            direction = Direction.Right;
            this.transform.localScale = new Vector3(1, 1, 1);
        } else
        {
            direction = Direction.Left;
            this.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private bool hitGround()
    {
        Vector2 frontPoint = (Vector2)transform.position + collider2D.offset + new Vector2((rigidbody2D.velocity.x > 0 ? 1 : -1), 0) * (collider2D.bounds.size.x * 0.5f);
        Vector2 upPoint = (Vector2)transform.position + collider2D.offset + Vector2.up * (collider2D.bounds.size.y * 0.5f);
        if (Physics2D.OverlapArea(upPoint, frontPoint, 1 << LayerMask.NameToLayer("Ground")) != null)
        {
            return true;
        }
        return false;
    }

    public bool nearPlatformBoundary()
    {
        float rayToGroundDistance = Mathf.Abs((Vector2.down * collider2D.bounds.size.y * 0.5f).y) + 0.5f;

        Vector3 frontPoint = (Vector2)transform.position + collider2D.offset + new Vector2((rigidbody2D.velocity.x > 0 ? 1 : -1), 0) * (collider2D.bounds.size.x * 0.5f);
        RaycastHit2D rayHit = Physics2D.Raycast(frontPoint, Vector2.down, 100, 1 << LayerMask.NameToLayer("Ground"));
        if (rayHit.distance > rayToGroundDistance)
        {
            return true;
        }
        return false;

    }
}
