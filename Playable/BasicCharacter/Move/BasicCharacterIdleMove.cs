using Common.Playable.Input;
using Common.Playable.Move;

namespace Common.Playable.BasicCharacter.Move;

public partial class BasicCharacterIdleMove : AMove
{

    protected override void TransitionLegsState(IInputPackage inputPackage, double delta)
    {
        //ChangeLegState(inputPackage.InputDirection != Vector2.Zero ? "Walk" : "Idle");
    }

    public override bool TracksPartialMove()
    {
        return true;
    }

    protected override (MoveStatus, string) DefaultLifeCycle(IInputPackage inputPackage)
    {
        return BestInputThatCanBePaid(inputPackage);
    }

    protected override void Update(IInputPackage inputPackage, double delta)
    {
    }
    
    public override void OnEnterState()
    {
    }

    public override void OnExitState()
    {
    }

    public override int GetPriority()
    {
        return 1;
    }
}