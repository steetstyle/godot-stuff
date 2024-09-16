using Godot;

namespace Common.Playable.Animation;

public partial class SplitBodyAnimator : Node
{
	[Export] public AnimationPlayer TorsoAnimationPlayer;
	[Export] public AnimationPlayer LegsAnimationPlayer;
	public Model Model;

	[Export] public bool FullBodyMode = true;
	[Export] public float SynchronizationDelta = 0.001f;

	public const string TorsoAnimationSuffix = "torso";
	public const string LegAnimationSuffix = "legs";

	public void UpdateBodyAnimations()
	{
		if (TorsoAnimationPlayer == null || LegsAnimationPlayer == null) return;
		UpdatePlayMode();
		UpdateAnimations();
	}

	public void UpdateLegAnimations()
	{
		if (TorsoAnimationPlayer == null || LegsAnimationPlayer == null) return;
		var currentMove = Model.GetCurrentMove();
		UpdatePlayMode();
		PlayLegAnimation(currentMove.CurrentLegMove.Animation);
	}

	private void UpdateAnimations()
	{
		if (TorsoAnimationPlayer == null || LegsAnimationPlayer == null) return;
		var currentMove = Model.GetCurrentMove();
		if (FullBodyMode)
		{
			if (LegsAnimationPlayer.CurrentAnimation == ConvertToTorsoAnimationName(currentMove.Animation)) return;
			PlayTorsoAnimation(currentMove.Animation);
			PlayLegAnimation(currentMove.Animation);
			Synchronize();
		}
		else
		{
			if (TorsoAnimationPlayer == null) return;
				if(TorsoAnimationPlayer.CurrentAnimation != ConvertToTorsoAnimationName(currentMove.Animation))
					PlayTorsoAnimation(currentMove.Animation);
			if (LegsAnimationPlayer == null) return;
				if (LegsAnimationPlayer.CurrentAnimation != ConvertToLegAnimationName(currentMove.Animation))
					PlayLegAnimation(currentMove.Animation);
		}
	}

	private static string ConvertToTorsoAnimationName(string animation)
	{
		return $"{animation}_{TorsoAnimationSuffix}";
	}
	
	
	private void PlayTorsoAnimation(string animation)
	{
		TorsoAnimationPlayer?.Play(ConvertToTorsoAnimationName(animation));
	}
	
	private static string ConvertToLegAnimationName(string animation)
	{
		return $"{animation}_{LegAnimationSuffix}";
	}

	private void PlayLegAnimation(string animation)
	{
		LegsAnimationPlayer?.Play(ConvertToLegAnimationName(animation));
	}
	
	private void Synchronize()
	{
		var synchronize = Mathf.Abs(TorsoAnimationPlayer.CurrentAnimationPosition - LegsAnimationPlayer.CurrentAnimationPosition) > SynchronizationDelta;
		if (synchronize) TorsoAnimationPlayer.Seek(LegsAnimationPlayer.CurrentAnimationPosition);
	}

	private void UpdatePlayMode()
	{
		var modelCurrentMove = Model.GetCurrentMove();
		FullBodyMode = !modelCurrentMove.TracksPartialMove();
	}

	public void SetSpeedScale(float speedScale)
	{
		if (TorsoAnimationPlayer == null || LegsAnimationPlayer == null) return;
		TorsoAnimationPlayer.SpeedScale = speedScale;
		LegsAnimationPlayer.SpeedScale = speedScale;
	}

	public void SetRootMotionTrack(string boneName)
	{
		TorsoAnimationPlayer.RootMotionTrack = boneName;
		LegsAnimationPlayer.RootMotionTrack = boneName;
	}
}
