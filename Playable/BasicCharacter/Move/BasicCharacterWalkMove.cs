using Common.Playable.Input;
using Common.Playable.Move;
using Godot;

namespace Common.Playable.BasicCharacter.Move;

public partial class BasicCharacterWalkMove : AMove
{
    [Export] public float Speed { get; set; } = 3.0f;
    [Export] public float TurnSpeed { get; set; } = 2.0f;

    protected override void TransitionLegsState(IInputPackage inputPackage, double delta)
    {
    }

    public override bool TracksPartialMove()
    {
        return true;
    }

    protected override void Update(IInputPackage inputPackage, double delta)
    {
        Humanoid.MoveAndSlide();
    }

    protected override (MoveStatus, string) DefaultLifeCycle(IInputPackage inputPackage)
    {
        return BestInputThatCanBePaid(inputPackage);
    }

    // Processes the input and adjusts humanoid's velocity and rotation.
    protected override void ProcessInputVector(IInputPackage inputPackage, double delta)
    {
        // Convert input direction to the humanoid's local space.
        var inputDirection = CameraMount.Basis
                             * new Vector3(-inputPackage.InputDirection.X, 0, -inputPackage.InputDirection.Y);
            inputDirection = inputDirection.Normalized();

        var faceDirection = Humanoid.Basis.Z;
        var angle = faceDirection.SignedAngleTo(inputDirection, Vector3.Up);

        // Adjust humanoid velocity and rotation based on the input and current facing direction.
       if (Mathf.Abs(angle) >= TrackingAngularSpeed * delta)
       {
           // Rotate gradually towards input direction
           Humanoid.Velocity = faceDirection.Rotated(Vector3.Up, (float)(Mathf.Sign(angle) * TrackingAngularSpeed * delta)) * Speed;
           Humanoid.RotateY((float)(Mathf.Sign(angle) * TrackingAngularSpeed * delta));
       }
       else
       {
           // Fully face the input direction and move forward
           Humanoid.Velocity = faceDirection.Rotated(Vector3.Up, angle) * Speed;
           Humanoid.RotateY(angle);
       }
        
        // Set animation speed based on velocity
        SplitBodyAnimator.SetSpeedScale(Humanoid.Velocity.Length() / Speed);
    }

    public override void OnEnterState()
    {
        //SplitBodyAnimator.SetRootMotionTrack("Character/Skeleton3D:mixamorig_Root");
    }

    // Reset the animator speed when exiting the state
    public override void OnExitState()
    {
        //SplitBodyAnimator.SetRootMotionTrack("");
        SplitBodyAnimator.SetSpeedScale(1);
    }
    
    public override int GetPriority()
    {
        return 2;
    }
}
