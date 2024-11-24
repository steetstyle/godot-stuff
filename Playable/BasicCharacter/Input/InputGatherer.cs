using Common.Playable.Input;
using Godot;

namespace Common.Playable.BasicCharacter.Input;

public partial class InputGatherer : Node, IInputGatherer<InputPackage>
{
    public InputPackage Gather()
    {
        var inputPackage = new InputPackage();
        inputPackage.Actions.Add("WalkStart");

       inputPackage.InputDirection = Godot.Input.GetVector("right", "left", "forward", "backward");
       if(inputPackage.InputDirection != Vector2.Zero)
           inputPackage.Actions.Add("WalkStart");

        return inputPackage;
    }
}
