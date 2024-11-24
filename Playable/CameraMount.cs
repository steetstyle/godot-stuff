using Godot;

namespace Common.Playable
{
    public partial class CameraMount : Node3D
    {
        private bool _mouseIsCaptured = true;
        [Export] public Camera3D PlayerCamera;
        [Export] public Node3D Player;
        [Export] public float CameraHeightOffset = 2f;  // Height offset to look at the center of the player
        [Export] public float MaxDistance = 5f;  // Maximum distance allowed from the player

        private Vector3 _startForward;
        private float _circleRadius = 1f; // Radius of the boundary
        private float _minDistance = 1.1f;  // Minimum distance from the player
        private float _maxCameraSpeed = 10f; // Max speed when the camera is far from the player
        private float _normalCameraSpeed = 2f; // Speed when near the player

        public override void _Ready()
        {
            Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
            TopLevel = true;

            _startForward = Player.GlobalTransform.Basis.Z; // Capture forward vector at start
        }

        public override void _Process(double delta)
        {
            var playerPosition = Player.GlobalPosition;
            playerPosition.Y += CameraHeightOffset;  // Aim at the player's center (height offset)

            // Calculate the distance between the camera and the player
            var distanceFromPlayer = GlobalPosition.DistanceTo(playerPosition);

            // Handle camera movement based on distance from player
            if (distanceFromPlayer > MaxDistance)
            {
                // If the camera is farther than MaxDistance, pull it back
                PullCameraBackToMaxDistance((float)delta, playerPosition);
            }
            else if (distanceFromPlayer > _circleRadius)
            {
                // If the player is far, increase speed to catch up
                MoveCameraTowardsPlayer((float)delta, playerPosition, distanceFromPlayer);
            }
            else if (distanceFromPlayer < _minDistance)
            {
                // If the camera is too close, move it back to maintain a minimum distance
                //MoveCameraAwayFromPlayer((float)delta, playerPosition);
            }
            else
            {
                // Smoothly adjust the camera to follow the player at normal speed
                LerpCameraToPlayer((float)delta, playerPosition);
            }

            // Rotate the camera smoothly to always face the player
            //SmoothCameraRotation(delta, playerPosition);
        }

        private void MoveCameraTowardsPlayer(float delta, Vector3 targetPosition, float distanceFromPlayer)
        {
            // Increase camera speed proportionally based on distance
            float speedFactor = Mathf.Clamp(distanceFromPlayer / _circleRadius, 1, _maxCameraSpeed);
            GlobalPosition = GlobalPosition.MoveToward(targetPosition, delta * speedFactor);
        }

        private void MoveCameraAwayFromPlayer(float delta, Vector3 targetPosition)
        {
            // Move the camera back slightly if it's too close to the player
            Vector3 awayDirection = (GlobalPosition - targetPosition).Normalized();
            GlobalPosition += awayDirection * _normalCameraSpeed * delta;
        }

        private void LerpCameraToPlayer(float delta, Vector3 targetPosition)
        {
            // Smoothly interpolate the camera's position to follow the player at a normal speed
            GlobalPosition = GlobalPosition.MoveToward(targetPosition, delta * _normalCameraSpeed);
        }

        private void PullCameraBackToMaxDistance(float delta, Vector3 targetPosition)
        {
            // If the camera exceeds the maximum distance, pull it back to the max allowed distance
            Vector3 directionToPlayer = (GlobalPosition - targetPosition).Normalized();
            GlobalPosition = targetPosition + directionToPlayer * MaxDistance;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionReleased("mouse_mode_switch"))
                SwitchMode();

            if (@event is not InputEventMouseMotion mouseMotion || !_mouseIsCaptured) return;

            // Horizontal camera rotation based on mouse movement
            var horizontalDirection = mouseMotion.Relative.X;
            RotateY(-horizontalDirection / 1000);
        }

        private void SwitchMode()
        {
            Godot.Input.MouseMode = _mouseIsCaptured ? Godot.Input.MouseModeEnum.Visible : Godot.Input.MouseModeEnum.Captured;
            _mouseIsCaptured = !_mouseIsCaptured;
        }
    }
}
