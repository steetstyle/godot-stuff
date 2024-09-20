using Common.Playable.Input;
using Common.Playable.Move;
using Godot;

namespace Common.Playable.Basic.Move;

public partial class BasicWalkMove : AMove
{
	[Export] public float Speed { get; set; } = 1.0f;
	[Export] public float TurnSpeed { get; set; } = 1.0f;
	[Export] public float LandingHeight = 2.163f;

	private float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

	protected override (MoveStatus, string) DefaultLifeCycle(IInputPackage inputPackage)
	{
		return !Humanoid.IsOnFloor() ? (MoveStatus.Next, "JumpIdle") : BestInputThatCanBePaid(inputPackage);
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
		ProcessInputVector(inputPackage, (float)delta);
		Humanoid.MoveAndSlide();
	}

	// Processes the input and adjusts humanoid's velocity and rotation.
	private void ProcessInputVector(IInputPackage inputPackage, float delta)
	{
		// Convert input direction to the humanoid's local space.
		var inputDirection = CameraMount.Basis
							 * new Vector3(-inputPackage.InputDirection.X, 0, -inputPackage.InputDirection.Y);
		inputDirection = inputDirection.Normalized();

		var faceDirection = Humanoid.Basis.Z;
		var angle = faceDirection.SignedAngleTo(inputDirection, Vector3.Up);

		// Adjust humanoid velocity and rotation based on the input and current facing direction.
		if (Mathf.Abs(angle) >= TurnSpeed * delta)
		{
			// Rotate gradually towards input direction
			Humanoid.Velocity = faceDirection.Rotated(Vector3.Up, Mathf.Sign(angle) * TurnSpeed * delta) * Speed;
			Humanoid.RotateY(Mathf.Sign(angle) * TurnSpeed * delta);
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

	// Reset the animator speed when exiting the state
	public override void OnExitState()
	{
		SplitBodyAnimator.SetSpeedScale(1);
	}

	public override int GetPriority()
	{
		return 2;
	}
}
