using System;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using RandomizerMod.FsmStateActions;

using CallStaticMethod = RandomizerMod.FsmStateActions.CallStaticMethod;

namespace RandomizerMod.Actions
{
    [Serializable]
    public class ChangeShinyIntoBigItem : RandomizerAction
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string objectName;
        [SerializeField] private string fsmName;
        [SerializeField] private string boolName;
        [SerializeField] private string spriteKey;
        [SerializeField] private string takeKey;
        [SerializeField] private string nameKey;
        [SerializeField] private string buttonKey;
        [SerializeField] private string descOneKey;
        [SerializeField] private string descTwoKey;

        public ChangeShinyIntoBigItem(string sceneName, string objectName, string fsmName, string boolName, string spriteKey, string takeKey, string nameKey, string buttonKey, string descOneKey, string descTwoKey)
        {
            this.sceneName = sceneName;
            this.objectName = objectName;
            this.fsmName = fsmName;
            this.boolName = boolName;
            this.spriteKey = spriteKey;
            this.takeKey = takeKey;
            this.nameKey = nameKey;
            this.buttonKey = buttonKey;
            this.descOneKey = descOneKey;
            this.descTwoKey = descTwoKey;
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
                        FsmState bigGetFlash = fsm.GetState("Big Get Flash");

                        //Remove actions that stop shiny from spawning
                        pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
                        pdBool.RemoveActionsOfType<StringCompare>();

                        //Add action to potentially despawn the object
                        pdBool.AddAction(new RandomizerBoolTest(boolName, null, "COLLECTED", true));

                        //Force the FSM to show the big item flash
                        charm.ClearTransitions();
                        charm.AddTransition("FINISHED", "Big Get Flash");

                        //Set bool and show the popup after the flash
                        bigGetFlash.AddAction(new RandomizerSetBool(boolName, true, true));
                        bigGetFlash.AddAction(new CallStaticMethod(typeof(BigItemPopup), "Show", new object[]
                        {
                            spriteKey,
                            takeKey,
                            nameKey,
                            buttonKey,
                            descOneKey,
                            descTwoKey,
                            fsm.gameObject,
                            "GET ITEM MSG END"
                        }));

                        //Exit the fsm after the popup
                        bigGetFlash.ClearTransitions();
                        bigGetFlash.AddTransition("GET ITEM MSG END", "Hero Up");

                        //Changes have been made, stop looping
                        break;
                    }
                }
            }
        }
    }
}
