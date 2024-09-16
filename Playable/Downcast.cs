using Godot;

namespace Common.Playable;

public partial class Downcast : RayCast3D
{
    [Export] public BoneAttachment3D RootAttachment;
    [Export] public CsgSphere3D CsgSphere3D;

    public override void _Process(double delta)
    {
        GlobalPosition = RootAttachment.GlobalPosition;
        CsgSphere3D.GlobalPosition = GetCollisionPoint();
    }
}