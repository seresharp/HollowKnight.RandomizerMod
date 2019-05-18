using HutongGames.PlayMaker;
using SeanprCore;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerBoolTest : FsmStateAction
    {
        private readonly string boolName;
        private readonly FsmEvent failEvent;
        private readonly bool playerdata;
        private readonly FsmEvent successEvent;

        public RandomizerBoolTest(string boolName, string failEventName, string successEventName,
            bool playerdata = false)
        {
            this.boolName = boolName;
            this.playerdata = playerdata;

            if (failEventName != null)
            {
                if (FsmEvent.EventListContains(failEventName))
                {
                    failEvent = FsmEvent.GetFsmEvent(failEventName);
                }
                else
                {
                    failEvent = new FsmEvent(failEventName);
                }
            }

            if (successEventName != null)
            {
                if (FsmEvent.EventListContains(successEventName))
                {
                    successEvent = FsmEvent.GetFsmEvent(successEventName);
                }
                else
                {
                    successEvent = new FsmEvent(successEventName);
                }
            }
        }

        public RandomizerBoolTest(string boolName, FsmEvent failEvent, FsmEvent successEvent, bool playerdata = false)
        {
            this.boolName = boolName;
            this.playerdata = playerdata;
            this.failEvent = failEvent;
            this.successEvent = successEvent;
        }

        public override void OnEnter()
        {
            if (playerdata && Ref.PD.GetBool(boolName) ||
                !playerdata && RandomizerMod.Instance.Settings.GetBool(false, boolName))
            {
                if (successEvent != null)
                {
                    Fsm.Event(successEvent);
                }
            }
            else
            {
                if (failEvent != null)
                {
                    Fsm.Event(failEvent);
                }
            }

            Finish();
        }
    }
}