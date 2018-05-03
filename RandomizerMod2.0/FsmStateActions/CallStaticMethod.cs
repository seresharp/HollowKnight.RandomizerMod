using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace RandomizerMod.FsmStateActions
{
    internal class CallStaticMethod : FsmStateAction
    {
        private MethodInfo info;
        private object[] parameters;

        public CallStaticMethod(Type t, string methodName, object[] parameters)
        {
            info = typeof(BigItemPopup).GetMethod("Show", BindingFlags.Static | BindingFlags.Public);

            if (info == null)
            {
                info = typeof(BigItemPopup).GetMethod("Show", BindingFlags.Static | BindingFlags.NonPublic);
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
                Modding.Logger.LogError("Error invoking static method from FSM:\n" + e);
            }
            Finish();
        }
    }
}
