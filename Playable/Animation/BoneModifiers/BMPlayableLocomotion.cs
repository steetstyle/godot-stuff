using Godot;
using Godot.Collections;

namespace Common.Playable.Animation.BoneModifiers;

public partial class BMPlayableLocomotion : BoneModifier
{
    private BMBlendSpace2D _animator = new();
    private Dictionary<Vector2, string> _directionSpectre;
    private Vector2 _newInputVector;


    public override void UpdateParameters(Vector2 inputVector)
    {
        _newInputVector = inputVector;
        _animator.UpdateParameters(inputVector);
    }

    public void Transition(Dictionary<Vector2, string> newDirectionSpectre)
    {
        _animator.Init(Animator, Skeleton);
        _directionSpectre = newDirectionSpectre;
        _animator.Transition(newDirectionSpectre);
    }

    public override Transform3D SuggestBonePose(int boneIndex)
    {
        return _animator.SuggestBonePose(boneIndex);
    }

    public override float GetBlendingPercentage()
    {
        return _animator.GetBlendingPercentage();
    }
}
