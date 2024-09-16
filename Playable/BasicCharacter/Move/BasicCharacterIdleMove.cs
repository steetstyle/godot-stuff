using Common.Playable.Input;
using Common.Playable.Move;

namespace Common.Playable.BasicCharacter.Move;

public partial class BasicCharacterIdleMove : AMove
{
    public override int Priority { get; init; } = 1;

    protected override void TransitionLegsState(IInputPackage inputPackage, double delta)
    {
        //ChangeLegState(inputPackage.InputDirection != Vector2.Zero ? "Walk" : "Idle");
    }

    public override bool TracksPartialMove()
    {
        return true;
    }

    protected override void Update(IInputPackage inputPackage, double delta)
    {
    }
    
    public override void OnEnterState()
    {
    }

    // Reset the animator speed when exiting the state
    public override void OnExitState()
    {
        SplitBodyAnimator.SetSpeedScale(1);
    }
}