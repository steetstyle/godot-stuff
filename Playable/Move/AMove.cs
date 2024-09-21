#nullable enable
using System;
using System.Linq;
using Common.Playable.Animation;
using Common.Playable.Input;
using Godot;

namespace Common.Playable.Move;
public enum MoveStatus
{
    Okay,
    Next,
}

public abstract partial class AMove : Node
{
    protected (MoveStatus, string?) BestInputThatCanBePaid(IInputPackage inputPackage)
    {
        inputPackage.Actions.Sort(Container.MovesPrioritySort);
        var actions = inputPackage.Actions.Where(action => Resource.CanBePaid(Container.Moves[action]));
        foreach (var action in actions)
        {
            return Container.Moves[action] == this ? (MoveStatus.Okay, null) : (MoveStatus.Next, action);
        }

        throw new Exception("Some reason Input.Actions doesn't contain even idle");
    }

    public (MoveStatus, string?) CheckRelevance(IInputPackage inputPackage)
    {
        if (!HasForcedMove) return DefaultLifeCycle(inputPackage);

        HasForcedMove = false;
        return (MoveStatus.Next, ForcedMove);
    }


    protected virtual (MoveStatus, string?) DefaultLifeCycle(IInputPackage inputPackage)
    {
        return WorksLongerThan(Duration) ? BestInputThatCanBePaid(inputPackage) : (MoveStatus.Okay, null);
    }

    public void _Update(IInputPackage inputPackage, double delta)
    {
        if (TracksPartialMove())
            TransitionLegsState(inputPackage, delta);
        UpdateResources(delta);
        if (TracksInputVector(delta))
            ProcessInputVector(inputPackage, delta);
        Update(inputPackage, delta);
    }

    protected abstract void TransitionLegsState(IInputPackage inputPackage, double delta);

    public abstract bool TracksPartialMove();

    protected abstract void Update(IInputPackage inputPackage, double delta);

    public void UpdateResources(double delta)
    {
        Resource.Update(delta);
    }

    protected void ChangeLegState(string moveName)
    {
        CurrentLegMove = Container.GetMoveByName(moveName);
        SplitBodyAnimator.UpdateLegAnimations();
    }

    public AMove CurrentLegMove { get; set; }

    private bool TracksInputVector(double delta)
    {
        return MoveRepository.TracksInputVector(BackendAnimation, GetProgress());
    }

    protected virtual void ProcessInputVector(IInputPackage inputPackage, double delta)
    { 
        var inputDirection =
            (CameraMount.Basis * new Vector3(-inputPackage.InputDirection.X, 0, -inputPackage.InputDirection.Y))
            .Normalized();
        var faceDirection = Humanoid.Basis.Z;
        var angle = faceDirection.SignedAngleTo(inputDirection, Vector3.Up);
        Humanoid.RotateY((float)Mathf.Clamp(angle, -TrackingAngularSpeed * delta, TrackingAngularSpeed * delta));
    }

    [Export] public float TrackingAngularSpeed = 10.0f;

    public virtual void OnExitState(){}
    public virtual void OnEnterState(){}
    public void MarkEnterState()
    {
        EnterStateTime = Time.GetUnixTimeFromSystem();
    }

    protected double GetProgress()
    {
        var now = Time.GetUnixTimeFromSystem();
        return now - EnterStateTime;
    }

    protected bool WorksLongerThan(double time)
    {
        return GetProgress() >= time;
    }

    protected bool WorksLessThan(double time)
    {
        return GetProgress() < time;
    }

    protected bool WorksBetween(double start, double end)
    {
        var progress = GetProgress();
        return start > progress && progress <= end;
    }

    public virtual float GetStaminaCost()
    {
        return 0.0f;
    }

    protected CharacterBody3D Humanoid { get; set; }
    protected SplitBodyAnimator SplitBodyAnimator { get; set; }
    protected Resource Resource { get; set; }
    protected HumanoidStates Container { get; set; }
    protected MoveRepository MoveRepository { get; set; }
    protected CameraMount CameraMount { get; set; }
    public double Duration { get; set; }
    [Export] public string Name { get; set; }
    [Export] public string BackendAnimation { get; set; }
    [Export] public string Animation { get; set; }
    [Export] public bool HasForcedMove { get; set; }
    [Export] public string? ForcedMove { get; set; }
    private double EnterStateTime { get; set; }

    public virtual int GetPriority()
    {
        return 0;
    }

    public void BuildMove(
        CharacterBody3D humanoid,
        SplitBodyAnimator splitBodyAnimator,
        Resource resource, 
        MoveRepository moveRepository, 
        CameraMount cameraMount,
        HumanoidStates container
    )
    {
        Humanoid = humanoid;
        SplitBodyAnimator = splitBodyAnimator;
        Resource = resource;
        MoveRepository = moveRepository;
        CameraMount = cameraMount;
        Container = container;
    }
}