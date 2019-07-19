using HutongGames.PlayMaker;
using JetBrains.Annotations;
using Modding;
using SeanprCore;

namespace RandomizerLib.FsmStateActions
{
    [PublicAPI]
    public class RandomizerSetBool : FsmStateAction
    {
        private readonly Mod _mod;
        private readonly string _name;
        private readonly bool _playerdata;
        private readonly bool _val;

        public RandomizerSetBool(Mod mod, string boolName, bool val, bool playerdata = false)
        {
            _mod = mod;
            _name = boolName;
            _val = val;
            _playerdata = playerdata;
        }

        public override void OnEnter()
        {
            if (_playerdata)
            {
                Ref.PD.SetBool(_name, _val);
            }
            else
            {
                _mod.SaveSettings.SetBool(_val, _name);
            }

            Finish();
        }
    }
}