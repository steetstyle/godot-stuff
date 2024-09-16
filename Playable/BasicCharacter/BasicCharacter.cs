using Godot;
using InputGatherer = Common.Playable.BasicCharacter.Input.InputGatherer;

namespace Common.Playable.BasicCharacter;

public partial class BasicCharacter : CharacterBody3D
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
