using Godot;

namespace Common.Playable;

public partial class CameraMount : Node3D
{
    private bool _mouseIsCaptured = true;
    [Export] public Camera3D PlayerCamera;
    [Export] public Node3D Player;
    
    public override void _Ready()
    {
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
        TopLevel = true;
    }

    public override void _Process(double delta)
    {
        GlobalPosition = Player.GlobalPosition;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionReleased("mouse_mode_switch"))
            SwitchMode();

        if (@event is not InputEventMouseMotion mouseMotion || !_mouseIsCaptured) return;

        var horizontalDirection = mouseMotion.Relative.X;
        RotateY(-horizontalDirection / 1000);
    }

    private void SwitchMode()
    {
        Godot.Input.MouseMode = _mouseIsCaptured ? Godot.Input.MouseModeEnum.Visible : Godot.Input.MouseModeEnum.Captured;
        _mouseIsCaptured = !_mouseIsCaptured;
    }
}