using System;
using UnityEngine;

using Object = UnityEngine.Object;

namespace RandomizerMod.Actions
{
    [Serializable]
    public abstract class RandomizerAction
    {
        public enum ActionType
        {
            GameObject,
            PlayMakerFSM
        }

        public abstract ActionType Type { get; }

        public static void Hook()
        {
            UnHook();

            On.PlayMakerFSM.OnEnable += ProcessFSM;
        }

        public static void UnHook()
        {
            On.PlayMakerFSM.OnEnable -= ProcessFSM;
        }

        public static void ProcessFSM(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM fsm)
        {
            orig(fsm);

            string scene = fsm.gameObject.scene.name;
            foreach (RandomizerAction action in RandomizerMod.Instance.Settings.actions)
            {
                if (action.Type == ActionType.PlayMakerFSM)
                {
                    try
                    {
                        action.Process(scene, fsm);
                    }
                    catch (Exception e)
                    {
                        RandomizerMod.Instance.LogError($"Error processing action of type {action.GetType()}:\n{JsonUtility.ToJson(action)}\n{e}");
                    }
                }
            }
        }

        public abstract void Process(string scene, Object changeObj);
    }
}
