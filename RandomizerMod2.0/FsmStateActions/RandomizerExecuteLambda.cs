using System;
using HutongGames.PlayMaker;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerExecuteLambda : FsmStateAction
    {
        private Action method;

        public RandomizerExecuteLambda(Action method)
        {
            this.method = method;
        }

        public override void OnEnter()
        {
            try
            {
                method();
            }
            catch (Exception e)
            {
                RandomizerMod.Instance.LogError("Error in RandomizerExecuteLambda:\n" + e);
            }

            Finish();
        }
    }
}
