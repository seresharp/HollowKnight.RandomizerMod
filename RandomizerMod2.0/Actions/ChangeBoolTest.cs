using System;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using RandomizerMod.Extensions;

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

        public ChangeBoolTest(string sceneName, string objectName, string fsmName, string stateName, string boolName)
        {
            this.sceneName = sceneName;
            this.objectName = objectName;
            this.fsmName = fsmName;
            this.stateName = stateName;
            this.boolName = boolName;
        }

        //Looping this much to change only one action is pretty bad
        //Use this class sparingly
        public override void Process()
        {
            if (GameManager.instance.GetSceneNameString() == sceneName)
            {
                foreach (PlayMakerFSM fsm in fsmList)
                {
                    if (fsm.FsmName == fsmName && fsm.gameObject.name == objectName)
                    {
                        fsm.GetState(stateName).GetActionsOfType<PlayerDataBoolTest>()[0].boolName = boolName;
                        break;
                    }
                }
            }
        }
    }
}
