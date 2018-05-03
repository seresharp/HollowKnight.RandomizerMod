using System;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;

namespace RandomizerMod.Actions
{
    [Serializable]
    public class ChangeShinyIntoTrinket : RandomizerAction
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string objectName;
        [SerializeField] private string fsmName;
        [SerializeField] private int trinketNum;

        public ChangeShinyIntoTrinket(string sceneName, string objectName, string fsmName, int trinketNum)
        {
            this.sceneName = sceneName;
            this.objectName = objectName;
            this.fsmName = fsmName;
            this.trinketNum = trinketNum;
        }

        public override void Process()
        {
            if (GameManager.instance.GetSceneNameString() == sceneName)
            {
                foreach (PlayMakerFSM fsm in fsmList)
                {
                    if (fsm.FsmName == fsmName && fsm.gameObject.name == objectName)
                    {
                        FsmState pdBool = fsm.GetState("PD Bool?");
                        FsmState charm = fsm.GetState("Charm?");
                        FsmState trinkFlash = fsm.GetState("Trink Flash");

                        //Remove actions that stop shiny from spawning
                        pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
                        pdBool.RemoveActionsOfType<StringCompare>();

                        //Force the FSM to follow the path for the correct trinket
                        charm.ClearTransitions();
                        charm.AddTransition("FINISHED", "Trink Flash");
                        trinkFlash.ClearTransitions();
                        trinkFlash.AddTransition("FINISHED", $"Trink {trinketNum}");

                        //Changes have been made, stop looping
                        break;
                    }
                }
            }
        }
    }
}
