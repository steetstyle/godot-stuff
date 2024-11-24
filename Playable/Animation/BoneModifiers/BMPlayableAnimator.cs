#nullable enable
using Godot;

namespace Common.Playable.Animation.BoneModifiers;
public partial class BMPlayableAnimator : BoneModifier
{
    private Godot.Animation _currentAnimation = null!;
    private double _currentAnimationProgress;
    private bool _currentAnimationCycling = true;

    private Godot.Animation? _nextAnimation;
    private double _nextAnimationProgress;
    private bool _nextAnimationCycling = true;

    private bool _isBlending;
    private double _blendDuration;
    private double _blendTimeSpent;
    private double _blendingPercentage;

    private double _lastProcessingTime;
    private double _deltaTime;

    private float _derivativeDelta = 0.002f;
    private int _rootPosTrack;
    private int _track;

    public void Play(string animationName, float blendDuration = 0)
    {
        if (_isBlending || _nextAnimation != null)
            FinalizeBlend();

        if (blendDuration > 0)
            StartBlend(animationName, blendDuration);
        else
        {
            _currentAnimation = Animator.GetAnimation(animationName);
            _currentAnimationCycling = _currentAnimation.LoopMode == Godot.Animation.LoopModeEnum.Linear;
        }
    }

    public void Transition(string targetAnimationName, float targetProgress, float overTime)
    {
        _nextAnimation = Animator.GetAnimation(targetAnimationName);
        _nextAnimationProgress = targetProgress;
        _nextAnimationCycling = _nextAnimation.LoopMode == Godot.Animation.LoopModeEnum.Linear;
        _blendDuration = overTime;
        _isBlending = true;
    }

    private void StartBlend(string animationName, float blendDuration)
    {
        _nextAnimation = Animator.GetAnimation(animationName);
        _nextAnimationProgress = 0;
        _nextAnimationCycling = _nextAnimation.LoopMode == Godot.Animation.LoopModeEnum.Linear;

        _blendingPercentage = 0;
        _blendTimeSpent = 0;
        _blendDuration = blendDuration;
        _isBlending = true;
    }

    private void FinalizeBlend()
    {
        _currentAnimation = _nextAnimation!;
        _currentAnimationProgress = _nextAnimationProgress;
        _currentAnimationCycling = _nextAnimationCycling;

        _isBlending = false;
        _blendingPercentage = 0;
        _blendTimeSpent = 0;
    }

    public override void UpdateParameters(Vector2 input)
    {
        UpdateTime();
        UpdateBlendValues();
    }

    private void UpdateTime()
    {
        var now = Time.GetUnixTimeFromSystem();
        _deltaTime = now - _lastProcessingTime;
        _lastProcessingTime = now;

        _currentAnimationProgress += _deltaTime;
        if (_currentAnimationCycling)
            _currentAnimationProgress = Mathf.PosMod(_currentAnimationProgress, _currentAnimation.Length);

        if (OnAnimationFinished != null && _currentAnimationProgress >= _currentAnimation.Length)
            OnAnimationFinished(_currentAnimation);

        if (_nextAnimation == null) return;
        _nextAnimationProgress += _deltaTime;
        if (_nextAnimationCycling)
            _nextAnimationProgress = Mathf.PosMod(_nextAnimationProgress, _nextAnimation.Length);
    }

    private void UpdateBlendValues()
    {
        if (!_isBlending || _nextAnimation == null) return;

        _blendTimeSpent += _deltaTime;
        _blendingPercentage = _blendTimeSpent / _blendDuration;

        if (_blendingPercentage >= 1)
            FinalizeBlend();
    }

    public override Transform3D SuggestBonePose(int boneIndex)
    {
        var transform = new Transform3D { Origin = Vector3.Inf, Basis = Basis.Identity };

        _track = _currentAnimation.FindTrack(BoneToTrackName(boneIndex), Godot.Animation.TrackType.Rotation3D);
        if (_track != -1)
            transform.Basis = BlendBasis();

        _track = _currentAnimation.FindTrack(BoneToTrackName(boneIndex), Godot.Animation.TrackType.Position3D);
        if (_track != -1)
            transform.Origin = BlendPosition();

        return transform;
    }

    private Basis BlendBasis()
    {
        if (_nextAnimation == null)
            return new Basis(_currentAnimation.RotationTrackInterpolate(_track, _currentAnimationProgress).Normalized());

        var currentRotation = _currentAnimation.RotationTrackInterpolate(_track, _currentAnimationProgress).Normalized();
        var nextRotation = _nextAnimation.RotationTrackInterpolate(_track, _nextAnimationProgress).Normalized();

        return new Basis(new Quaternion(
            Mathf.Lerp(currentRotation.X, nextRotation.X, (float)_blendingPercentage),
            Mathf.Lerp(currentRotation.Y, nextRotation.Y, (float)_blendingPercentage),
            Mathf.Lerp(currentRotation.Z, nextRotation.Z, (float)_blendingPercentage),
            Mathf.Lerp(currentRotation.W, nextRotation.W, (float)_blendingPercentage)
        ).Normalized());
    }

    private Vector3 BlendPosition()
    {
        if (_nextAnimation == null)
            return _currentAnimation.PositionTrackInterpolate(_track, _currentAnimationProgress);

        var currentPosition = _currentAnimation.PositionTrackInterpolate(_track, _currentAnimationProgress);
        var nextPosition = _nextAnimation.PositionTrackInterpolate(_track, _nextAnimationProgress);

        return currentPosition.Lerp(nextPosition, (float)_blendingPercentage);
    }

    public override void Init(AnimationPlayer animator, Skeleton3D skeleton)
    {
        base.Init(animator, skeleton);
      
        _currentAnimationCycling = false;
        _currentAnimationProgress = 0;
        _lastProcessingTime = Time.GetUnixTimeFromSystem();
    }

    protected override Vector3 CalculateRootVelocity()
    {
        var adjustmentDelta = Time.GetUnixTimeFromSystem() - _lastProcessingTime;
        return _nextAnimation != null ? CalculateVelocity(_currentAnimation, _currentAnimationProgress, adjustmentDelta).Lerp(
               CalculateVelocity(_nextAnimation, _nextAnimationProgress, adjustmentDelta), 
               (float)_blendingPercentage) : CalculateVelocity(_currentAnimation, _currentAnimationProgress, adjustmentDelta);
    }

    private Vector3 CalculateVelocity(Godot.Animation animation, double progress, double adjustmentDelta)
    {
        var now = Mathf.PosMod(progress + adjustmentDelta, animation.Length);
        var past = Mathf.Max(now - _derivativeDelta, 0);
        var future = Mathf.Min(now + _derivativeDelta, animation.Length);

        var pastPosition = animation.PositionTrackInterpolate(_rootPosTrack, past);
        var futurePosition = animation.PositionTrackInterpolate(_rootPosTrack, future);

        return (futurePosition - pastPosition) / (float)(future - past);
    }

    public override float GetBlendingPercentage()
    {
        return (float)_blendingPercentage;
    }

    public string? GetCurrentAnimation()
    {
        return _currentAnimation == null ? null : _currentAnimation.GetName();
    }
}
