using System;
using HutongGames.PlayMaker;

namespace RandomizerLib.FsmStateActions
{
    public class RandomizerExecuteLambda : FsmStateAction
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
                LogHelper.LogError("Error in RandomizerExecuteLambda:\n" + e);
            }

            Finish();
        }
    }
}