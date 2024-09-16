using Godot;

namespace Common.Playable.Move;

public partial class MoveRepository: Node
{
	[Export] public MoveData MoveData;
	public float GetDuration(string animation)
	{
		return MoveData.GetAnimation(animation).Length;
	}

	public bool TracksInputVector(string animation, double progress)
	{
		var data = MoveData.GetAnimation(animation);
		var track = data.FindTrack("MoveData:TracksInputVector", Godot.Animation.TrackType.Value);
		return MoveData.GetBooleanValue(animation, track, progress);
	}
}
