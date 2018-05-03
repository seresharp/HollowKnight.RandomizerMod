using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerSetBool : FsmStateAction
    {
        private string name;
        private bool val;
        private bool playerdata;

        public RandomizerSetBool(string boolName, bool val, bool playerdata = false)
        {
            name = boolName;
            this.val = val;
            this.playerdata = playerdata;
        }

        public override void OnEnter()
        {
            if (playerdata) PlayerData.instance.SetBool(name, val);
            else RandomizerMod.instance.Settings.SetBool(val, name);

            Finish();
        }
    }
}
