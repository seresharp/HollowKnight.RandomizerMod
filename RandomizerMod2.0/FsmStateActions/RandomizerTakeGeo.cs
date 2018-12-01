using HutongGames.PlayMaker;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerTakeGeo : FsmStateAction
    {
        private int amount;

        public RandomizerTakeGeo(int amount)
        {
            this.amount = amount;
        }

        public override void OnEnter()
        {
            if (amount > 0 && amount <= PlayerData.instance?.geo)
            {
                HeroController.instance?.TakeGeo(amount);
            }

            Finish();
        }
    }
}
