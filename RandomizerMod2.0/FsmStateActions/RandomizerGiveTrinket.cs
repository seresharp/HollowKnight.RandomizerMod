using HutongGames.PlayMaker;

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
            PlayerData.instance.SetBool($"foundTrinket{num}", true);
            PlayerData.instance.SetBool($"noTrinket{num}", false);
            PlayerData.instance.IncrementInt($"trinket{num}");

            Finish();
        }
    }
}