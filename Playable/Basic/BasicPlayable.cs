using Common.Playable.Basic.Input;
using Godot;
using InputGatherer = Common.Playable.BasicCharacter.Input.InputGatherer;

namespace Common.Playable.Basic;

public partial class BasicPlayable : CharacterBody3D
{
	[Export] public InputGatherer InputGatherer;
	[Export] public Model Model;

	public override void _Ready()
	{
		AddToGroup("players");
		InputGatherer ??= GetNode<InputGatherer>("InputGatherer");
		Model ??= GetNode<Model>("Model");
	}

	public override void _PhysicsProcess(double delta)
	{
		var inputPackage = InputGatherer.Gather();
		Model.Update(inputPackage, delta);
		inputPackage.QueueFree();
	}
}
