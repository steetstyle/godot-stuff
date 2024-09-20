using Common.Playable.Input;
using Common.Playable.Move;
using Godot;

namespace Common.Playable.Basic.Move;

public partial class BasicJumpIdleMove : AMove
{
    [Export] public Downcast Downcast;
    [Export] public float LandingHeight = 2.163f;
    [Export] public float DeltaVectorLength = 6.0f;
    private float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    private bool _jumped;
    private Vector3 _jumpDirection;

    protected override (MoveStatus, string) DefaultLifeCycle(IInputPackage inputPackage)
    {
        var floorPoint = Downcast.GetCollisionPoint();
        if (!(Downcast.RootAttachment.GlobalPosition.DistanceTo(floorPoint) < LandingHeight))
            return (MoveStatus.Okay, null);
        var xzVelocity = Humanoid.Velocity with { Y = 0 };
        return xzVelocity.LengthSquared() >= 10 ? (MoveStatus.Next, "Run") : BestInputThatCanBePaid(inputPackage);
    }

    protected override void TransitionLegsState(IInputPackage inputPackage, double delta)
    {
    }

    public override bool TracksPartialMove()
    {
        return false;
    }

    protected override void Update(IInputPackage inputPackage, double delta)
    {
        Humanoid.Velocity = Humanoid.Velocity with { Y = Humanoid.Velocity.Y - _gravity * (float)delta };
        Humanoid.MoveAndSlide();
    }

    protected override void ProcessInputVector(IInputPackage inputPackage, double delta)
    {
        var inputDirection =
            (CameraMount.Basis * new Vector3(-inputPackage.InputDirection.X, 0, -inputPackage.InputDirection.Y))
            .Normalized();
        var inputDeltaVector = inputDirection * DeltaVectorLength;

        _jumpDirection = (_jumpDirection + inputDeltaVector * (float)delta).LimitLength(Mathf.Clamp(Humanoid.Velocity.Length(), 1, 999999));
        Humanoid.LookAt(Humanoid.GlobalPosition - _jumpDirection);
        
        var newVelocity = (Humanoid.Velocity + inputDeltaVector * (float)delta).LimitLength(Humanoid.Velocity.Length());
        Humanoid.Velocity = newVelocity;
    }

    public override void OnEnterState()
    {
        _jumpDirection = Humanoid.Basis.Z * Mathf.Clamp(Humanoid.Velocity.Length(), 1, 999999);
        _jumpDirection.Y = 0;
    }

    public override int GetPriority()
    {
        return 10;
    }
}

