using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace RandomizerMod.Extensions
{
    internal static class PlayMakerFSMExtensions
    {
        private static FieldInfo fsmStringParams = typeof(ActionData).GetField("fsmStringParams", BindingFlags.NonPublic | BindingFlags.Instance);

        public static List<FsmString> GetStringParams(this ActionData self)
        {
            return (List<FsmString>)fsmStringParams.GetValue(self);
        }

        public static FsmState GetState(this PlayMakerFSM self, string name)
        {
            foreach (FsmState state in self.FsmStates)
            {
                if (state.Name == name) return state;
            }

            return null;
        }

        public static void RemoveActionsOfType<T>(this FsmState self)
        {
            List<FsmStateAction> actions = new List<FsmStateAction>();

            foreach (FsmStateAction action in self.Actions)
            {
                if (!(action is T))
                {
                    actions.Add(action);
                }
            }

            self.Actions = actions.ToArray();
        }

        public static T[] GetActionsOfType<T>(this FsmState self) where T : FsmStateAction
        {
            List<T> actions = new List<T>();

            foreach (FsmStateAction action in self.Actions)
            {
                if (action is T)
                {
                    actions.Add((T)action);
                }
            }

            return actions.ToArray();
        }

        public static void ClearTransitions(this FsmState self)
        {
            self.Transitions = new FsmTransition[0];
        }

        public static void AddTransition(this FsmState self, string eventName, string toState)
        {
            List<FsmTransition> transitions = self.Transitions.ToList();

            FsmTransition trans = new FsmTransition();
            trans.ToState = toState;

            if (FsmEvent.EventListContains(eventName))
            {
                trans.FsmEvent = FsmEvent.GetFsmEvent(eventName);
            }
            else
            {
                trans.FsmEvent = new FsmEvent(eventName);
            }

            transitions.Add(trans);

            self.Transitions = transitions.ToArray();
        }

        public static void AddAction(this FsmState self, FsmStateAction action)
        {
            List<FsmStateAction> actions = self.Actions.ToList();
            actions.Add(action);
            self.Actions = actions.ToArray();
        }
    }
}
