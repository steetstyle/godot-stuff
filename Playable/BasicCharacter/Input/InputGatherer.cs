using Common.Playable.Input;
using Godot;

namespace Common.Playable.BasicCharacter.Input;

public partial class InputGatherer : Node, IInputGatherer<InputPackage>
{
    [Export] public Camera3D PlayerCamera; // Assignable Camera from the editor
    [Export] public Node3D Player; // Assignable Player from the editor
    [Export] public float RayLength = 100f; // Length of the ray for mouse input detection
    [Export] public float MinimumDistance = 0.5f; // Minimum distance threshold to avoid glitches

    public InputPackage Gather()
    {
        var inputPackage = new InputPackage();
        inputPackage.Actions.Add("Walk");
        
        // Get the target point based on the mouse position
        var (_, targetPoint, collisionObject) = GetMouseTargetPosition();

        // Ensure a valid target was clicked on
        if (targetPoint == Vector3.Zero || collisionObject == null) return inputPackage;

        // Calculate the direction from the player to the target point
        inputPackage.MouseTargetPoint = targetPoint - Player.GlobalPosition;
        inputPackage.DirectionToTarget = inputPackage.MouseTargetPoint.Normalized();
        inputPackage.InputDirection = new Vector2(-inputPackage.DirectionToTarget.X, -inputPackage.DirectionToTarget.Z);

        // Calculate distance and check if we are within movement range
        var distanceToTarget = (Player.GlobalPosition with { Y = 0 }).DistanceTo(targetPoint with { Y = 0 });
        if (distanceToTarget <= MinimumDistance) return inputPackage;

        // Switch to "Run" if the point is not behind the player
        if (IsPointBehindPlayer(inputPackage.DirectionToTarget)) return inputPackage;

        inputPackage.Actions.Remove("Walk");
        inputPackage.Actions.Add("Run");

        return inputPackage;
    }

    private (Vector2, Vector3, Node3D) GetMouseTargetPosition()
    {
        var mousePosition = GetViewport().GetMousePosition();

        // Ray from the camera to the mouse position
        var rayOrigin = PlayerCamera.ProjectRayOrigin(mousePosition);
        var rayDirection = PlayerCamera.ProjectRayNormal(mousePosition) * RayLength;

        var spaceState = PlayerCamera.GetWorld3D().DirectSpaceState;
        var queryParams = new PhysicsRayQueryParameters3D
        {
            From = rayOrigin,
            To = rayOrigin + rayDirection,
            CollisionMask = 1
        };

        var rayResult = spaceState.IntersectRay(queryParams);

        if (rayResult.Count > 0)
        {
            var targetPosition = (Vector3)rayResult["position"];
            var collidedObject = rayResult["collider"].Obj as Node3D;
            return (mousePosition, targetPosition, collidedObject);
        }

        return (mousePosition, Vector3.Zero, null);
    }

    private bool IsPointBehindPlayer(Vector3 directionToTarget)
    {
        var playerForward = Player.GlobalTransform.Basis.Z.Normalized();
        return playerForward.Dot(directionToTarget) < 0;
    }
}
