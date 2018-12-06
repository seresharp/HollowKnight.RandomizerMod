using System;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using RandomizerMod.FsmStateActions;
using UnityEngine;

namespace RandomizerMod.Actions
{
    [Serializable]
    internal class ChangeBoolTest : RandomizerAction
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string objectName;
        [SerializeField] private string fsmName;
        [SerializeField] private string stateName;
        [SerializeField] private string boolName;
        [SerializeField] private bool playerdata;

        public ChangeBoolTest(string sceneName, string objectName, string fsmName, string stateName, string boolName, bool playerdata = false)
        {
            this.sceneName = sceneName;
            this.objectName = objectName;
            this.fsmName = fsmName;
            this.stateName = stateName;
            this.boolName = boolName;
            this.playerdata = playerdata;
        }

        // Looping this much to change only one action is pretty bad
        // Use this class sparingly
        public override void Process()
        {
            if (GameManager.instance.GetSceneNameString() == sceneName)
            {
                foreach (PlayMakerFSM fsm in FsmList)
                {
                    if (fsm.FsmName == fsmName && fsm.gameObject.name == objectName)
                    {
                        PlayerDataBoolTest pdBoolTest = fsm.GetState(stateName).GetActionsOfType<PlayerDataBoolTest>()[0];

                        if (playerdata)
                        {
                            pdBoolTest.boolName = boolName;
                        }
                        else
                        {
                            RandomizerBoolTest boolTest = new RandomizerBoolTest(boolName, pdBoolTest.isFalse, pdBoolTest.isTrue);
                            fsm.GetState(stateName).RemoveActionsOfType<PlayerDataBoolTest>();
                            fsm.GetState(stateName).AddFirstAction(boolTest);
                        }

                        break;
                    }
                }
            }
        }
    }
}
