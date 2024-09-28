using Common.Playable.BasicCharacter.Input;
using Common.Playable.Input;
using Common.Playable.Move;
using Godot;

namespace Common.Playable.BasicCharacter.Move;

public partial class BasicCharacterWalkMove : AMove
{
    [Export] public float Speed { get; set; } = 3.0f;
    private const float MaxRotationAngle = 45; // Max allowed angle relative to the road forward direction

    protected override void ProcessInputVector(IInputPackage inputPackage, double delta)
    {
        if (inputPackage is not InputPackage basicInputPackage) return;

        // Raycast to find the forward vector of the surface below the character
        var surfaceForwardVector = GetSurfaceForwardVector();
        if (surfaceForwardVector == Vector3.Zero)
        {
            // If no road detected, fallback to default forward movement
            surfaceForwardVector = Humanoid.GlobalTransform.Basis.Z.Normalized();
        }
        
        // Calculate direction from humanoid to mouse target point
        var mouseTargetDirection = (basicInputPackage.MouseTargetPoint - Humanoid.GlobalTransform.Origin).Normalized();

        // Calculate the angle between the surface forward vector and the desired movement direction
        var angleBetween = surfaceForwardVector.Normalized().AngleTo(mouseTargetDirection);
        
        var humanoidRight = Humanoid.GlobalTransform.Basis.X.Normalized();
        var rotationAxis = surfaceForwardVector.Cross(humanoidRight).Normalized();

        // Ensure the rotation axis is valid (non-zero)
        if (rotationAxis.Length() > 0.001) // Check if axis has a non-zero length
        {
            var targetHumanoidForward = Humanoid.GlobalTransform.Basis.Rotated(rotationAxis, angleBetween);

            if (IsRotationProper(surfaceForwardVector, targetHumanoidForward.Z.Normalized()))
            {
                // Rotate the humanoid by the angle between its forward and the target direction
                Humanoid.RotateY(angleBetween);

                // Update velocity based on the new forward direction after rotation
                Humanoid.Velocity = Humanoid.GlobalTransform.Basis.Z.Normalized() * Speed;
            }
        }   
        
        // Set animation speed based on velocity
        SplitBodyAnimator.SetSpeedScale(Humanoid.Velocity.Length() / Speed);
    }

    private bool IsRotationProper(Vector3 surfaceForward, Vector3 playerForward)
    {
        // Normalize the vectors to ensure correct angle calculations
        surfaceForward = surfaceForward.Normalized();
        playerForward = playerForward.Normalized();

        // Convert MaxRotationAngle to radians for comparison
        var maxSurfaceRotationAngleRad = Mathf.DegToRad(MaxRotationAngle);

        // Calculate the angle between the surface forward and player forward vectors
        var angleBetween = surfaceForward.AngleTo(playerForward);

        // Print the calculated angle
        GD.Print("Max Allowable Angle: ", Mathf.RadToDeg(maxSurfaceRotationAngleRad), "\t Actual Angle: ", Mathf.RadToDeg(angleBetween));

        // Check if the angle is within the allowable range (between -MaxRotationAngle and MaxRotationAngle)
        return Mathf.Abs(angleBetween) <= maxSurfaceRotationAngleRad;
    }


    // Raycast below the character to get the surface forward vector
    private Vector3 GetSurfaceForwardVector()
    {
        // Cast a ray from the character's position downwards to find the road's forward direction
        var from = Humanoid.GlobalTransform.Origin;
        var to = from + Vector3.Down * 10f; // Arbitrary length of ray to detect ground

        // Perform raycast
        var spaceState = Humanoid.GetWorld3D().DirectSpaceState;
        var query = new PhysicsRayQueryParameters3D
        {
            From = from,
            To = to,
            CollisionMask = 1
        };

        var result = spaceState.IntersectRay(query);

        // Get the collision object and check if it has a forward direction (road mesh)
        if (result.Count <= 0) return Vector3.Zero;

        if (result["collider"].Obj is Node3D collidedObject)
        {
            // Return the forward vector of the collided object
            return Vector3.Forward;
            return collidedObject.GlobalTransform.Basis.Z.Normalized();
        }

        // Return Zero vector if no collision found (indicating no valid surface)
        return Vector3.Zero;
    }

    protected override void TransitionLegsState(IInputPackage inputPackage, double delta) {}

    public override bool TracksPartialMove() => true;

    protected override void Update(IInputPackage inputPackage, double delta)
    {
        Humanoid.MoveAndSlide();
    }

    public override void OnEnterState() {}

    public override void OnExitState()
    {
        SplitBodyAnimator.SetSpeedScale(1);
    }

    public override int GetPriority()
    {
        return 2;
    }
}
