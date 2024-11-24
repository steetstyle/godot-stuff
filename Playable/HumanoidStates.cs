using System.Collections.Generic;
using Common.Playable.Animation;
using Common.Playable.Move;
using Godot;

namespace Common.Playable;


public partial class HumanoidStates : Node
{
    public CharacterBody3D Humanoid;
    public TransitionModifier MainAnimator;
    public Resource Resource;
    [Export] public MoveRepository MoveRepository;
    [Export] public CameraMount CameraMount;

    public readonly Dictionary<string, AMove> Moves = new();
   
    public void AcceptMoves()
    {
        
        foreach (var children in GetChildren())
        {
            if (children is not AMove move) continue;

            move.BuildMove(
                Humanoid,
                Resource,
                MoveRepository,
                CameraMount,
                this,
                MainAnimator
            );
            move.Duration = MoveRepository.GetDuration(move.BackendAnimation);
            Moves.Add(move.Name, move);
        }
    }

    public int MovesPrioritySort(string a, string b)
    {
        return Moves[b].GetPriority() - Moves[a].GetPriority();
    }

    public AMove GetMoveByName(string moveName)
    {
        return Moves[moveName];
    }
}