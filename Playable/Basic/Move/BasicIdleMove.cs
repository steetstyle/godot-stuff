using Common.Playable.Input;
using Common.Playable.Move;

namespace Common.Playable.Basic.Move;

public partial class BasicIdleMove : AMove
{
    public override int Priority { get; init; } = 1;

    protected override (MoveStatus, string) DefaultLifeCycle(IInputPackage inputPackage)
    {
         return !Humanoid.IsOnFloor() ? (MoveStatus.Next, "JumpIdle") : BestInputThatCanBePaid(inputPackage);
    }

    protected override void TransitionLegsState(IInputPackage inputPackage, double delta)
    {
    }

    public override bool TracksPartialMove()
    {
        return false;
    }

    protected override void Update(IInputPackage inputPackage, double delta)
    {
    }
}