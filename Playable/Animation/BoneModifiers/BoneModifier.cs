using System;
using Godot;

namespace Common.Playable.Animation.BoneModifiers;

public partial class BoneModifier : Resource
{
    protected AnimationPlayer Animator;
    protected Skeleton3D Skeleton;
    public Action<Godot.Animation> OnAnimationFinished = _ => {};

    protected Vector2 LastInputVector;
    
    public void SetInputVector(Vector2 inputVector)
    {
        LastInputVector = inputVector;
    }

    public virtual Transform3D SuggestBonePose(int boneIndex)
    {
        return new Transform3D();
    }

    public virtual void UpdateParameters(Vector2 inputVector)
    {
        LastInputVector = inputVector;
    }

    protected virtual Vector3 CalculateRootVelocity()
    {
        return Vector3.Zero;
    }

    protected string BoneToTrackName(int boneIndex)
    {
        var boneName = Skeleton.GetBoneName(boneIndex);
        return "root/Skeleton3D:" + boneName;
    }

    public virtual void Init(AnimationPlayer animator, Skeleton3D skeleton)
    {
        Animator = animator;
        Skeleton = skeleton;
    }

    public virtual float GetBlendingPercentage()
    {
        return 0.0f;
    }
}