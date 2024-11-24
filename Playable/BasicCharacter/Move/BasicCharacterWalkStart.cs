using Common.Playable.Input;
using Common.Playable.Move;
using Godot;
using Godot.Collections;

namespace Common.Playable.BasicCharacter.Move;

public partial class BasicCharacterWalkStart : AMove
{
    private readonly Dictionary<Vector2, string> _directionSpectre = new()
    {
        { new Vector2(0, 0), "W2_Stand_Relaxed_Fgt_v1_IPC" }, // Idle
        { new Vector2(0, 1), "W2_Stand_Relaxed_To_Walk_F_IPC" }, // Forward
        { new Vector2(0, -1), "W2_Stand_Relaxed_To_Walk_B_IPC" }, // Backward
        { new Vector2(-1, 0), "W2_Stand_Relaxed_To_Walk_L_IPC" }, // Left
        { new Vector2(1, 0), "W2_Stand_Relaxed_To_Walk_R_IPC" }, // Right
        { new Vector2(-1, 1), "W2_Stand_Relaxed_To_Walk_L_IPC" }, // Forward-Left
        { new Vector2(1, 1), "W2_Stand_Relaxed_To_Walk_R_IPC" }, // Forward-Right
        { new Vector2(-1, -1), "W2_Stand_Relaxed_To_Walk_L_IPC" }, // Backward-Left
        { new Vector2(1, -1), "W2_Stand_Relaxed_To_Walk_R_IPC" }, // Backward-Right
    };

    [Export] public float Speed { get; set; } = 3.0f;

    protected override void ProcessInputVector(IInputPackage inputPackage, double delta)
    {
        Humanoid.Velocity = Humanoid.GlobalTransform.Basis.Z.Normalized() * Speed;
    }

    protected override void Update(IInputPackage inputPackage, double delta)
    {
        Humanoid.MoveAndSlide();
        MainAnimator.SetLocomotionInput(inputPackage.InputDirection);
    }

    public override void Animate()
    {
        MainAnimator.TransitionToLocomotion(_directionSpectre);
    }

    public override int GetPriority()
    {
        return 2;
    }
}
