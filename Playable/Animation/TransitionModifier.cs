#nullable enable
using System;
using System.Collections.Generic;
using Common.Playable.Animation.BoneModifiers;
using Godot;

namespace Common.Playable.Animation;
[Tool]
[GlobalClass]
public partial class TransitionModifier : SkeletonModifier3D
{
    private Skeleton3D _skeleton = null!;
    private AnimationPlayer _animator = null!;
    
    private readonly IDictionary<BoneModifierEnum, BoneModifier> _states = new Godot.Collections.Dictionary<BoneModifierEnum, BoneModifier>();
    
    private BoneModifier? _currentState;
    private Transform3D _currentTransform;

    private BoneModifier? _nextState;
    private Transform3D _nextTransform;

    private Vector2 _lastInput;
    private BMPlayableLocomotion? _nextLocomotion;

    public override void _Ready()
    {
        base._Ready();
        _skeleton = GetSkeleton();
        _animator = _skeleton.GetNode<AnimationPlayer>("../../AnimationPlayer");
        InitializeModifiers();
    }

    private void InitializeModifiers()
    {
        _states[BoneModifierEnum.Animator] = new BMPlayableAnimator();
        _states[BoneModifierEnum.Locomotion] = new BMPlayableLocomotion();

        foreach (var modifier in _states.Values)
        {
            modifier.Init(_animator, _skeleton);
            (modifier as BMPlayableAnimator)?.Transition("W2_Stand_Relaxed_Fgt_v1_IPC", 0, 0);
        }
    }
    
    public override void _ProcessModification()
    {
        if (_currentState == null) return;
        UpdateBoneTransforms();
    }
    

    private void UpdateBoneTransforms()
    {
        if (_currentState == null) return;
        _currentState.UpdateParameters(_lastInput);
        _nextState?.UpdateParameters(_lastInput);

        for (var boneIndex = 0; boneIndex < _skeleton.GetBoneCount(); boneIndex++)
        {
            if (_nextState != null && _currentState != _nextState)
                ApplyBlendedTransform(boneIndex);
            else
                ApplyCurrentTransform(boneIndex);
        }
    }

    private void ApplyBlendedTransform(int boneIndex)
    {
        if (_nextState == null || _currentState == null) return;
        _currentTransform = _currentState.SuggestBonePose(boneIndex);
        var fBlendingPercentage = _currentState.GetBlendingPercentage();
        _nextTransform = _nextState.SuggestBonePose(boneIndex);
        _skeleton.SetBonePoseRotation(boneIndex, 
            _currentTransform.Basis.GetRotationQuaternion()
                .Slerp(_nextTransform.Basis.GetRotationQuaternion(), fBlendingPercentage));
        _skeleton.SetBonePosePosition(boneIndex,
            _currentTransform.Origin != Vector3.Inf
                ? _currentTransform.Origin.Lerp(_nextTransform.Origin, fBlendingPercentage)
                : Vector3.Zero);
    }

    private void ApplyCurrentTransform(int boneIndex)
    {
        if (_currentState == null) return;
        _currentTransform = _currentState.SuggestBonePose(boneIndex);
        _skeleton.SetBonePoseRotation(boneIndex, _currentTransform.Basis.GetRotationQuaternion());
        if (_currentTransform.Origin != Vector3.Inf)
            _skeleton.SetBonePosePosition(boneIndex, _currentTransform.Origin);
    }

    private void Transition(BoneModifierEnum targetModifier, double blendingDuration)
    {
        if (blendingDuration > 0)
        {
            _nextState = _states[targetModifier];
            _currentState ??= _states[targetModifier];
        }
        else
            _currentState = _states[targetModifier];
    }

    public void SetLocomotionInput(Vector2 input)
    {
        (_currentState as BMPlayableLocomotion)?.SetInputVector(input);
        (_nextState as BMPlayableLocomotion)?.SetInputVector(input);
    }
    
    public void TransitionToAnimator(string targetAnimation, float targetTime, float blendingDuration = 0, Action<Godot.Animation>? onAnimationFinished = null)
    {
        Transition(BoneModifierEnum.Animator, blendingDuration);
        var animator = (_currentState as BMPlayableAnimator)!;
        animator.OnAnimationFinished = onAnimationFinished;

        if (targetTime == 0)
            animator.Play(targetAnimation);
        else
            animator.Transition(targetAnimation, targetTime, blendingDuration);
            
    }

    public void TransitionToLocomotion(Godot.Collections.Dictionary<Vector2, string> newDirectionSpectre)
    {
        Transition(BoneModifierEnum.Locomotion, 0);
        (_currentState as BMPlayableLocomotion)?.Transition(newDirectionSpectre);
    }
}
