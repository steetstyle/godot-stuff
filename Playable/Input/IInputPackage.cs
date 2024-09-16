using System.Collections.Generic;
using Godot;

namespace Common.Playable.Input;

public interface IInputPackage
{
    List<string> Actions { get; set; }
    Vector2 InputDirection { get; set; }
}