using System.Collections.Generic;
using Common.Playable.Animation;
using Common.Playable.Move;
using Godot;

namespace Common.Playable;


public partial class HumanoidStates : Node
{
    [Export] public CharacterBody3D Humanoid;
    [Export] public SplitBodyAnimator SplitBodyAnimator;
    [Export] public Resource Resource;
    [Export] public MoveRepository MoveRepository;
    [Export] public CameraMount CameraMount;

    public readonly Dictionary<string, AMove> Moves = new();
   
    public void AcceptMoves()
    {
        foreach (var children in GetChildren())
        {
            if (children is not AMove move) continue;

            move.Humanoid = Humanoid;
            move.SplitBodyAnimator = SplitBodyAnimator;
            move.Resource = Resource;
            move.MoveRepository = MoveRepository;
            move.CameraMount = CameraMount;
            move.Container = this;
            move.Duration = MoveRepository.GetDuration(move.BackendAnimation);
            Moves.Add(move.Name, move);
        }
    }

    public int MovesPrioritySort(string a, string b)
    {
        return Moves[b].Priority - Moves[a].Priority;
    }

    public AMove GetMoveByName(string moveName)
    {
        return Moves[moveName];
    }
}