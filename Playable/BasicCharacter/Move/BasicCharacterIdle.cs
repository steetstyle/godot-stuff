using System;
using System.Collections.Generic;
using Common.Playable.Input;
using Common.Playable.Move;

namespace Common.Playable.BasicCharacter.Move
{
    public partial class BasicCharacterIdle : AMove
    {
        private static readonly List<string> IdleAnimations = new()
        {
            "W2_Stand_Relaxed_Fgt_v1_IPC",
            "W2_Stand_Relaxed_Fgt_v2_IPC",
            "W2_Stand_Relaxed_Fgt_v3_IPC",
        };

        private static readonly Random RandomGenerator = new();

        protected override (MoveStatus, string) DefaultLifeCycle(IInputPackage inputPackage)
        {
            return BestInputThatCanBePaid(inputPackage);
        }

        protected override void Update(IInputPackage inputPackage, double delta)
        {
        }

        public override void OnEnterState()
        {
            var randomAnimation = IdleAnimations[RandomGenerator.Next(IdleAnimations.Count)];
            MainAnimator.TransitionToAnimator(randomAnimation, 0f, 0.3f);
        }

        public override void OnExitState()
        {
        }

        public override int GetPriority()
        {
            return 1;
        }
    }
}