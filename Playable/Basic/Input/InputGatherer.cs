using Common.Playable.Input;
using Godot;

namespace Common.Playable.Basic.Input;

public partial class InputGatherer : Node, IInputGatherer<FootballPlayer.Input.InputPackage>
{
	public FootballPlayer.Input.InputPackage Gather()
	{
		var inputPackage = new FootballPlayer.Input.InputPackage();
		
		inputPackage.Actions.Add("Idle");

		inputPackage.InputDirection = Godot.Input.GetVector("right", "left", "forward", "backward");
		if(inputPackage.InputDirection != Vector2.Zero)
			inputPackage.Actions.Add(Godot.Input.IsActionPressed("sprint") ? "Run" : "Walk");

		if(Godot.Input.IsActionPressed("jump"))
			inputPackage.Actions.Add("JumpStart");
		
		return inputPackage;
	}
}
