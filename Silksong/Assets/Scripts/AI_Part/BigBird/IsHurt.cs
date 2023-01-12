namespace BehaviorDesigner.Runtime.Tasks
{
    public class IsHurt :Conditional
    {
        public bool _b = false;
        public override void OnStart()
        {
            BigBirdController _controller = Owner.GetComponentInChildren<BigBirdController>();
        }

        public override TaskStatus OnUpdate()
        {
            if (_b)
            {
                return TaskStatus.Failure;
            }
            else
            {
                return TaskStatus.Success;
            }
            
        }
    }
}