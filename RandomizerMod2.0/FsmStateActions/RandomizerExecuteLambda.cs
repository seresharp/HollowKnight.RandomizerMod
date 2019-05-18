using System;
using HutongGames.PlayMaker;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerExecuteLambda : FsmStateAction
    {
        private readonly Action _method;

        public RandomizerExecuteLambda(Action method)
        {
            _method = method;
        }

        public override void OnEnter()
        {
            try
            {
                _method();
            }
            catch (Exception e)
            {
                LogError("Error in RandomizerExecuteLambda:\n" + e);
            }

            Finish();
        }
    }
}