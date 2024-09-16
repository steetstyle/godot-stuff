using Godot;

namespace Common.Playable.Move;

public partial class MoveData : AnimationPlayer
{
    [Export] public bool TracksInputVector;

    public bool GetBooleanValue(string animation, int track, double timeCode)
    {
        var data = GetAnimation(animation);
        var interpolate = data.ValueTrackInterpolate(track, timeCode);
        var value = interpolate.AsBool();
        return value;
    }
}