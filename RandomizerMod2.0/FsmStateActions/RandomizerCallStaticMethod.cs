using System;
using System.Reflection;
using HutongGames.PlayMaker;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerCallStaticMethod : FsmStateAction
    {
        private MethodInfo info;
        private object[] parameters;

        public RandomizerCallStaticMethod(Type t, string methodName, object[] parameters)
        {
            info = t.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);

            if (info == null)
            {
                info = t.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
            }

            if (info == null)
            {
                throw new ArgumentException($"Class {t} has no static method {methodName}");
            }

            this.parameters = parameters;
        }

        public override void OnEnter()
        {
            try
            {
                info.Invoke(null, parameters);
            }
            catch (Exception e)
            {
                RandomizerMod.instance.LogError("Error invoking static method from FSM:\n" + e);
            }
            Finish();
        }
    }
}
