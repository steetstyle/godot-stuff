using Common.Playable.Input;
using Common.Playable.Move;

namespace Common.Playable.Basic.Move;

public partial class BasicJumpStartMove : AMove
{
    private const float Speed = 3.0f;
    private const float VerticalSpeedAdded = 20.0f;

    private bool _jumped;

    protected override (MoveStatus, string) DefaultLifeCycle(IInputPackage inputPackage)
    {
         if (WorksLongerThan(Duration)) return (MoveStatus.Next, "JumpIdle");
        _jumped = false;
        return (MoveStatus.Okay, null);
    }

    protected override void TransitionLegsState(IInputPackage inputPackage, double delta)
    {
    }

    public override bool TracksPartialMove()
    {
        return false;
    }

    protected override void Update(IInputPackage inputPackage, double deltaTime)
    {
        ProcessJump(inputPackage);
        Humanoid.MoveAndSlide();
    }

    private void ProcessJump(IInputPackage inputPackage)
    {
        if (WorksLongerThan(0.3f) && _jumped)
        {
            Humanoid.Velocity = (Humanoid.Basis.Z * Speed) with { Y = Humanoid.Velocity.Y + VerticalSpeedAdded };
            _jumped = true;
        }
    }

    public override void OnEnterState()
    {
        Humanoid.Velocity = Humanoid.Velocity.Normalized() * Speed;
    }

    public override int GetPriority()
    {
        return 10;
    }
}
