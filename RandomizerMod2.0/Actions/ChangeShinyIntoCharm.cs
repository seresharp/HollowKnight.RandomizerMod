using System;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using RandomizerMod.FsmStateActions;

namespace RandomizerMod.Actions
{
    [Serializable]
    public class ChangeShinyIntoCharm : RandomizerAction
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string objectName;
        [SerializeField] private string fsmName;
        [SerializeField] private int charmNum;
        [SerializeField] private string boolName;

        public ChangeShinyIntoCharm(string sceneName, string objectName, string fsmName, int charmNum)
        {
            this.sceneName = sceneName;
            this.objectName = objectName;
            this.fsmName = fsmName;
            this.charmNum = charmNum;

            boolName = $"gotCharm_{charmNum}";
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
                        FsmState getCharm = fsm.GetState("Get Charm");

                        //Remove actions that stop shiny from spawning
                        pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
                        pdBool.RemoveActionsOfType<StringCompare>();

                        //Add action to potentially despawn the object
                        pdBool.AddAction(new RandomizerBoolTest(boolName, null, "COLLECTED", true));

                        //Force the FSM into the charm state, set it to the correct charm
                        charm.ClearTransitions();
                        charm.AddTransition("FINISHED", "Get Charm");
                        getCharm.RemoveActionsOfType<SetPlayerDataBool>();
                        getCharm.AddAction(new RandomizerSetBool(boolName, true, true));
                        getCharm.GetActionsOfType<SetFsmInt>()[0].setValue = charmNum;

                        //Changes have been made, stop looping
                        break;
                    }
                }
            }
        }
    }
}
