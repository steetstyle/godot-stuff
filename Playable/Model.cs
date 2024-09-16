using Common.Playable.Input;
using Common.Playable.Move;
using Godot;

namespace Common.Playable;

public partial class Model : Node
{
	[Export] public bool IsEnemy;
	[Export] public CharacterBody3D Playable;
	[Export] public HumanoidStates HumanoidStates;
	[Export] public Resource Resource;
	private AMove _currentMove;

	public override void _Ready()
	{
		Resource ??= GetNode<Resource>("Resource");
		Playable ??= GetNode<CharacterBody3D>("..");

		HumanoidStates.Humanoid = Playable;
		HumanoidStates.AcceptMoves();
		_currentMove = HumanoidStates.GetMoveByName("Idle");
		HumanoidStates.SplitBodyAnimator.Model = this;
		EnterMove();
	}

	public AMove GetCurrentMove()
	{
		return _currentMove;
	}

	public void Update(IInputPackage inputPackage, double delta)
	{
		var (next, nextAnimation) = _currentMove.CheckRelevance(inputPackage);
		if (next is MoveStatus.Next)
			SwitchTo(nextAnimation);
		_currentMove._Update(inputPackage, delta);
	}

	private void SwitchTo(string animation)
	{
		_currentMove?.OnExitState();
		_currentMove = HumanoidStates.Moves[animation];
		EnterMove();
	}

	private void EnterMove()
	{
		_currentMove.OnEnterState();
		_currentMove.MarkEnterState();
		Resource.Pay(_currentMove);
		HumanoidStates.SplitBodyAnimator.UpdateBodyAnimations();
	}
}
