using HutongGames.PlayMaker;
using JetBrains.Annotations;
using Modding;
using SeanprCore;

namespace RandomizerLib.FsmStateActions
{
    [PublicAPI]
    public class RandomizerBoolTest : FsmStateAction
    {
        private Mod _mod;
        private readonly string _boolName;
        private readonly FsmEvent _failEvent;
        private readonly bool _playerdata;
        private readonly FsmEvent _successEvent;

        public RandomizerBoolTest(Mod mod, string boolName, string failEventName, string successEventName,
            bool playerdata = false)
        {
            _mod = mod;
            _boolName = boolName;
            _playerdata = playerdata;

            if (failEventName != null)
            {
                _failEvent = FsmEvent.EventListContains(failEventName)
                    ? FsmEvent.GetFsmEvent(failEventName)
                    : new FsmEvent(failEventName);
            }

            if (successEventName == null)
            {
                return;
            }

            _successEvent = FsmEvent.EventListContains(successEventName)
                ? FsmEvent.GetFsmEvent(successEventName)
                : new FsmEvent(successEventName);
        }

        public RandomizerBoolTest(Mod mod, string boolName, FsmEvent failEvent, FsmEvent successEvent, bool playerdata = false)
        {
            _mod = mod;
            _boolName = boolName;
            _playerdata = playerdata;
            _failEvent = failEvent;
            _successEvent = successEvent;
        }

        public override void OnEnter()
        {
            if (_playerdata && Ref.PD.GetBool(_boolName) ||
                !_playerdata && _mod.SaveSettings.GetBool(false, _boolName))
            {
                if (_successEvent != null)
                {
                    Fsm.Event(_successEvent);
                }
            }
            else
            {
                if (_failEvent != null)
                {
                    Fsm.Event(_failEvent);
                }
            }

            Finish();
        }
    }
}