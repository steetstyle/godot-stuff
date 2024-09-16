using Godot;

namespace Common.Playable.Move;

public partial class MoveData : AnimationPlayer
{
    [Export] public Vector3 RootPosition;
    [Export] public bool TracksInputVector;
    [Export] public bool TransitionToBeQueued;
    [Export] public bool AcceptsQueueing;
    [Export] public bool AcceptsTrackingDuration;
    [Export] public bool IsVulnerable;
    [Export] public bool IsGraspable;

    public bool GetBooleanValue(string animation, int track, double timeCode)
    {
        var data = GetAnimation(animation);
        var interpolate = data.ValueTrackInterpolate(track, timeCode);
        var value = interpolate.AsBool();
        return value;
    }
}