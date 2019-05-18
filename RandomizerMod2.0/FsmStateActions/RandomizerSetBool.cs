using HutongGames.PlayMaker;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerSetBool : FsmStateAction
    {
        private readonly string name;
        private readonly bool playerdata;
        private readonly bool val;

        public RandomizerSetBool(string boolName, bool val, bool playerdata = false)
        {
            name = boolName;
            this.val = val;
            this.playerdata = playerdata;
        }

        public override void OnEnter()
        {
            if (playerdata)
            {
                PlayerData.instance.SetBool(name, val);
            }
            else
            {
                RandomizerMod.Instance.Settings.SetBool(val, name);
            }

            Finish();
        }
    }
}