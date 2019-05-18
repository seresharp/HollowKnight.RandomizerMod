using HutongGames.PlayMaker;
using SeanprCore;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerTakeGeo : FsmStateAction
    {
        private readonly int amount;

        public RandomizerTakeGeo(int amount)
        {
            this.amount = amount;
        }

        public override void OnEnter()
        {
            if (amount > 0 && amount <= Ref.PD?.geo)
            {
                Ref.Hero?.TakeGeo(amount);
            }

            Finish();
        }
    }
}