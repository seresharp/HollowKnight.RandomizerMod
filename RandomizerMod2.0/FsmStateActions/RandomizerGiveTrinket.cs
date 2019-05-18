using HutongGames.PlayMaker;
using SeanprCore;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerGiveTrinket : FsmStateAction
    {
        private readonly int num;

        public RandomizerGiveTrinket(int trinketNum)
        {
            num = trinketNum;
        }

        public override void OnEnter()
        {
            Ref.PD.SetBool($"foundTrinket{num}", true);
            Ref.PD.SetBool($"noTrinket{num}", false);
            Ref.PD.IncrementInt($"trinket{num}");

            Finish();
        }
    }
}