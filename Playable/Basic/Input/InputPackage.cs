using System.Collections.Generic;
using Common.Playable.Input;
using Godot;

namespace Common.Playable.Basic.Input;

public partial class InputPackage : Node, IInputPackage
{
	public List<string> Actions { get; set; } = new();
	public Vector2 InputDirection { get; set; }  = Vector2.Zero;
}
